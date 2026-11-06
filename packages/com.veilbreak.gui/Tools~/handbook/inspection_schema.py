"""Turn the kit's TypeScript declarations into data for the preview inspector.

This deliberately supports the declaration grammar used by the generated kit;
unknown types fail the build instead of silently dropping controls.
"""

import ast
import re


def split_top(text, separator):
    parts, start, stack, quote, escaped = [], 0, [], None, False
    for index, char in enumerate(text):
        if quote:
            if escaped:
                escaped = False
            elif char == '\\':
                escaped = True
            elif char == quote:
                quote = None
            continue
        if char in '"\'`':
            quote = char
        elif char in '{[(':
            stack.append(char)
        elif char in '}])':
            stack.pop()
        elif char == separator and not stack:
            parts.append(text[start:index].strip())
            start = index + 1
    parts.append(text[start:].strip())
    return [part for part in parts if part]


def block_end(text, start):
    depth, quote, escaped, comment = 0, None, False, False
    for index in range(start, len(text)):
        char = text[index]
        if comment:
            comment = char != '\n'  # a // comment may hold an apostrophe
        elif quote:
            if escaped:
                escaped = False
            elif char == '\\':
                escaped = True
            elif char == quote:
                quote = None
        elif char == '/' and text[index : index + 2] == '//' and text[index - 1] != ':':
            comment = True
        elif char in '"\'`':
            quote = char
        elif char == '{':
            depth += 1
        elif char == '}':
            depth -= 1
            if depth == 0:
                return index
    raise ValueError('Unclosed declaration block')


def build_schema(declarations, kit_source):
    text = re.sub(r'/\*.*?\*/|//[^\n]*', '', declarations, flags=re.S)
    types = {}
    for match in re.finditer(r'(?:export\s+)?interface\s+(\w+)\s*\{', text):
        start = match.end() - 1
        types[match[1]] = text[start : block_end(text, start) + 1]
    for match in re.finditer(r'(?:export\s+)?type\s+(\w+)\s*=\s*', text):
        types[match[1]] = split_top(text[match.end() :], ';')[0]

    def parse(expression, depth=0):
        expression = expression.strip()
        if depth > 20:
            raise ValueError('Recursive inspector type: ' + expression)
        # A callback is an action, not an editable data value.
        if expression.startswith('(') and '=>' in expression:
            return {'kind': 'action', 'type': expression}
        union = split_top(expression, '|')
        if len(union) > 1:
            nullable = 'null' in union
            union = [part for part in union if part != 'null']
            if len(union) == 1:
                return dict(parse(union[0], depth + 1), nullable=nullable)
            if all(re.fullmatch(r"'[^']*'|-?\d+(?:\.\d+)?", part) for part in union):
                return {'kind': 'enum', 'values': [ast.literal_eval(part) for part in union], 'nullable': nullable}
            return {'kind': 'union', 'choices': [parse(part, depth + 1) for part in union], 'nullable': nullable}
        intersection = split_top(expression, '&')
        if len(intersection) > 1:
            properties = {}
            for part in intersection:
                properties.update(parse(part, depth + 1)['properties'])
            return {'kind': 'object', 'properties': properties}
        if expression.endswith('[]'):
            return {'kind': 'array', 'items': parse(expression[:-2], depth + 1)}
        indexed = re.fullmatch(r"(\w+)\['(\w+)'\]", expression)
        if indexed:
            return parse(types[indexed[1]], depth + 1)['properties'][indexed[2]]
        if expression.startswith('{'):
            properties = {}
            for field in split_top(expression[1:-1], ';'):
                match = re.fullmatch(r'(\w+)(\?)?\s*:\s*(.*)', field, re.S)
                if not match:
                    raise ValueError('Unsupported inspector field: ' + field)
                properties[match[1]] = dict(parse(match[3], depth + 1), optional=bool(match[2]), type=match[3])
            return {'kind': 'object', 'properties': properties}
        if expression in ('boolean', 'number', 'string'):
            return {'kind': expression}
        if expression == 'React.ReactNode':
            return {'kind': 'node'}
        if expression == 'React.CSSProperties':
            return {'kind': 'json', 'format': 'css'}
        if re.fullmatch(r"'[^']*'|-?\d+(?:\.\d+)?", expression):
            return {'kind': 'enum', 'values': [ast.literal_eval(expression)]}
        if expression in types:
            result = parse(types[expression], depth + 1)
            if expression == 'KitIconName':
                result = dict(result, format='icon')
            return result
        raise ValueError('Unsupported inspector type: ' + expression)

    components = {}
    for match in re.finditer(r'export declare function (\w+)\(props\??:\s*(.*?)\): React.ReactElement;', text, re.S):
        components[match[1]] = parse(match[2])
    components['VbShape'] = parse(
        '{ as?: string; className?: string; style?: React.CSSProperties; children?: React.ReactNode }'
    )
    exports = kit_source.split('var api = {', 1)[1].split('};', 1)[0]
    exported = re.findall(r'(\w+): \w+', exports)
    objects = set(re.findall(r'export declare const (\w+)', text))  # VbEdgeFx, Glitch: helper objects, not components
    for name in exported:
        if not name[0].isupper() or name in objects:
            continue
        if name not in components:
            raise ValueError('Missing inspector component declaration: ' + name)
        # Include properties used by the runtime even if declarations lag behind.
        function = re.search(r'function ' + re.escape(name) + r'\(p\)\s*\{', kit_source)
        if function:
            start = function.end() - 1
            body = kit_source[start : block_end(kit_source, start) + 1]
            for prop in sorted(set(re.findall(r'\bp\.(\w+)', body))):
                components[name]['properties'].setdefault(
                    prop, {'kind': 'runtime', 'optional': True, 'type': 'Runtime property'}
                )
    return {'components': components}
