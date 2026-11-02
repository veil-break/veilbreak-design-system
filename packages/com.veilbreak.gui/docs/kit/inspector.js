/* Handbook-only React adapter. It preserves the rendered DOM and uses public
   Context/ref APIs; the distributable kit and React itself are never patched. */
(function () {
  'use strict';
  var R = window.React, create = R.createElement;
  var Owner = R.createContext(null), Scope = R.createContext(null);
  var records = new Map(), owners = new WeakMap(), nativeWrappers = new Map(), wrappers = new WeakMap(), names = new WeakMap();
  var sequence = 0, enabled = false, scope = null, selected = null, hovered = null, target = null;
  var treeFrame = 0, boundsFrame = 0, repaint = function () {}, metadata, panel, tree, detail, overlay, feedback, overrideSheet;
  var sectionContainer = null;
  var computedProperties = ['display', 'position', 'width', 'height', 'box-sizing', 'padding', 'margin', 'gap', 'color', 'background-color', 'border', 'font-family', 'font-size', 'font-weight', 'line-height', 'opacity', 'transform', 'overflow', 'pointer-events'];
  function element(tag, attributes, text) {
    var node = document.createElement(tag);
    Object.keys(attributes || {}).forEach(function (key) { node.setAttribute(key, attributes[key]); });
    if (text != null) node.textContent = text;
    return node;
  }
  function round(value) { return Math.round(value * 10) / 10; }
  function say(message) { if (feedback) feedback.textContent = message; }
  function serializable(value) {
    if (typeof value === 'function' || R.isValidElement(value)) return undefined;
    if (Array.isArray(value)) return value.map(serializable);
    if (value && typeof value === 'object') {
      var out = {}; Object.keys(value).forEach(function (key) { var next = serializable(value[key]); if (next !== undefined) out[key] = next; }); return out;
    }
    return value;
  }
  function at(object, path) { return path.reduce(function (value, key) { return value == null ? undefined : value[key]; }, object); }
  function assign(object, path, value, remove) {
    var key = path[0], next = Array.isArray(object) ? object.slice() : Object.assign({}, object);
    if (path.length === 1) { if (remove) delete next[key]; else next[key] = value; }
    else next[key] = assign(object && object[key] != null ? object[key] : typeof path[1] === 'number' ? [] : {}, path.slice(1), value, remove);
    return next;
  }
  function notify() {
    if (!enabled || treeFrame) return;
    treeFrame = requestAnimationFrame(function () {
      treeFrame = 0; if (!enabled) return;
      renderTree();
      if (selected && !records.has(selected.id)) select(null);
      else if (selected) { syncEdits(); if (!target || !target.isConnected) target = roots(selected)[0] || null; }
    });
  }
  function belongs(record, parent) {
    for (var current = record; current; current = current.parent) if (current === parent) return true;
    return false;
  }
  function roots(record) {
    var nodes = new Set();
    records.forEach(function (candidate) { if (belongs(candidate, record)) candidate.nodes.forEach(function (node) { if (node.isConnected) nodes.add(node); }); });
    return Array.from(nodes).filter(function (node) { return !nodes.has(node.parentElement); });
  }
  function native(type) {
    if (nativeWrappers.has(type)) return nativeWrappers.get(type);
    var Wrapped = R.forwardRef(function (props, forwarded) {
      var owner = R.useContext(Owner), current = R.useRef(null), id = R.useRef('n' + (++sequence));
      var ref = R.useCallback(function (node) {
        if (current.current && owner) owner.nodes.delete(current.current);
        current.current = node;
        if (node && owner) { owner.nodes.add(node); owners.set(node, owner); }
        if (typeof forwarded === 'function') forwarded(node); else if (forwarded) forwarded.current = node;
        notify();
      }, [owner, forwarded]);
      return create(type, Object.assign({}, props, { ref: ref }, owner ? { 'data-inspect-node': id.current } : {}));
    });
    Wrapped.displayName = 'PreviewElement(' + type + ')'; nativeWrappers.set(type, Wrapped); return Wrapped;
  }
  class Recovery extends R.Component {
    constructor(props) { super(props); this.state = { error: null }; }
    static getDerivedStateFromError(error) { return { error: error.message }; }
    componentDidCatch(error) { this.props.record.error = error.message; say('Preview could not render this value: ' + error.message + '. Reset the component or change the value.'); notify(); }
    render() { return this.state.error ? create('p', { role: 'alert' }, 'Preview could not render this value. Change it in the inspector or reset the component.') : this.props.children; }
  }
  function Scene(props) {
    var version = R.useState(0), ref = R.useRef(null);
    if (!ref.current) ref.current = { id: 'c' + (++sequence), name: 'Preview scene', nodes: new Set(), css: {}, states: {}, variants: {}, attributes: {}, props: {}, source: {}, parent: null, preview: props.preview };
    var record = ref.current;
    record.reset = function () { record.css = {}; record.states = {}; record.variants = {}; record.attributes = {}; version[1](function (value) { return value + 1; }); };
    R.useLayoutEffect(function () { records.set(record.id, record); notify(); return function () { records.delete(record.id); notify(); }; }, [record]);
    return create(Owner.Provider, { value: record }, create(R.Fragment, { key: version[0] }, props.node));
  }
  function component(type) {
    if (wrappers.has(type)) return wrappers.get(type);
    var Wrapped = R.forwardRef(function (props, forwarded) {
      var preview = R.useContext(Scope), parent = R.useContext(Owner), ref = R.useRef(null);
      var state = R.useState([]), edits = state[0], setEdits = state[1], version = R.useState(0);
      if (!ref.current) ref.current = { id: 'c' + (++sequence), name: names.get(type), nodes: new Set(), css: {}, states: {}, variants: {}, attributes: {}, preview: preview, parent: parent };
      var record = ref.current;
      record.source = props; record.preview = preview; record.parent = parent;
      var current = props;
      edits.forEach(function (edit) { current = assign(current, edit.path, edit.value, edit.remove); });
      record.props = current;
      record.apply = function (path, value, remove) {
        record.error = null;
        window.ReactDOM.flushSync(function () {
          setEdits(function (previous) { return previous.filter(function (edit) { return JSON.stringify(edit.path) !== JSON.stringify(path); }).concat([{ path: path, value: value, remove: !!remove }]); });
          version[1](function (value) { return value + 1; });
        });
      };
      record.reset = function () { record.css = {}; record.states = {}; record.variants = {}; record.attributes = {}; record.error = null; setEdits([]); version[1](function (value) { return value + 1; }); };
      R.useLayoutEffect(function () {
        if (!preview) return;
        records.set(record.id, record); notify();
        return function () { records.delete(record.id); notify(); };
      }, [preview, record]);
      R.useLayoutEffect(notify);
      var child = create(type, Object.assign({}, current, forwarded ? { ref: forwarded } : {}));
      return preview ? create(Owner.Provider, { value: record }, create(Recovery, { key: version[0], record: record }, child)) : child;
    });
    Wrapped.displayName = 'PreviewComponent(' + names.get(type) + ')'; wrappers.set(type, Wrapped); return Wrapped;
  }
  function factory(type, props) {
    var next = typeof type === 'string' ? native(type) : names.has(type) ? component(type) : type;
    return create.apply(null, [next, props].concat(Array.prototype.slice.call(arguments, 2)));
  }
  function install(api, useFactory, schedulePaint) {
    Object.keys(api).forEach(function (name) { var type = api[name]; if (typeof type === 'function' || (type && type.$$typeof)) names.set(type, name); });
    useFactory(factory); repaint = schedulePaint;
  }
  function pick(node) {
    for (var current = node; current; current = current.parentElement) {
      var record = owners.get(current);
      if (record && record.preview === scope) {
        // Shape is an implementation detail of the button/frame at the same root.
        if (record.name === 'VbShape' && record.parent) record = record.parent;
        return record;
      }
    }
    return null;
  }
  function renderTree() {
    var old = tree.value, fragment = document.createDocumentFragment();
    function visit(parent) { records.forEach(function (record) {
      if (record.preview !== scope || record.parent !== parent) return;
      var depth = 0; for (var ancestor = record.parent; ancestor; ancestor = ancestor.parent) depth++;
      var label = record.props.title || record.props.text || (record.props.view && record.props.view.name);
      var option = element('option', { value: record.id }, Array(depth + 1).join('  ') + record.name + (typeof label === 'string' ? ' · ' + label.slice(0, 45) : ''));
      fragment.appendChild(option); visit(record);
    }); }
    visit(null);
    tree.replaceChildren(fragment); tree.value = selected ? selected.id : old;
    if (!selected && tree.options.length) { var first = Array.from(tree.options).find(function (option) { return records.get(option.value).name !== 'Preview scene'; }) || tree.options[0]; select(records.get(first.value)); }
  }
  function box(record) {
    var nodes = roots(record), rects = nodes.map(function (node) { return node.getBoundingClientRect(); }).filter(function (rect) { return rect.width || rect.height; });
    if (!rects.length) return null;
    var left = Math.min.apply(null, rects.map(function (rect) { return rect.left; })), top = Math.min.apply(null, rects.map(function (rect) { return rect.top; }));
    return { x: left, y: top, width: Math.max.apply(null, rects.map(function (rect) { return rect.right; })) - left, height: Math.max.apply(null, rects.map(function (rect) { return rect.bottom; })) - top };
  }
  function drawRect(id, record) {
    var shape = document.getElementById(id), rect = record && box(record);
    shape.setAttribute('visibility', rect ? 'visible' : 'hidden');
    if (!rect) return;
    Object.keys(rect).forEach(function (key) { var value = String(round(rect[key])); if (shape.getAttribute(key) !== value) shape.setAttribute(key, value); });
    if (id === 'inspection-selection') {
      var output = document.getElementById('inspection-bounds');
      var text = 'Viewport: x ' + round(rect.x) + ', y ' + round(rect.y) + ' · ' + round(rect.width) + ' × ' + round(rect.height) + ' px';
      if (target) text += ' · Layout: ' + target.offsetWidth + ' × ' + target.offsetHeight + ' px';
      if (output && output.textContent !== text) output.textContent = text;
      var caption = document.getElementById('inspection-caption');
      caption.setAttribute('x', Math.max(4, rect.x)); caption.setAttribute('y', Math.max(14, rect.y - 6));
      caption.textContent = record.name + ' · ' + round(rect.width) + ' × ' + round(rect.height);
    }
  }
  function frame() {
    if (!enabled) return;
    drawRect('inspection-selection', selected); drawRect('inspection-hover', hovered);
    boundsFrame = requestAnimationFrame(frame);
  }
  function selectedNode() { return target && target.isConnected ? target : selected ? roots(selected)[0] : null; }
  function cssRules(node) {
    var result = [], unavailable = [];
    function visit(rules, source, conditions, active) {
      Array.from(rules || []).forEach(function (rule) {
        if (rule.selectorText && rule.style) {
          var matches = false;
          try { matches = node.matches(rule.selectorText.replace(/::(before|after)\b/g, '')); } catch (error) { /* Invalid/unsupported selectors cannot match this browser. */ }
          if (matches) result.push({ selector: rule.selectorText, css: rule.style.cssText, source: source, context: conditions.join(' → '), active: active });
        } else if (rule.cssRules) {
          var current = active, condition = rule.conditionText || rule.name || '';
          if (rule.type === CSSRule.MEDIA_RULE) current = current && matchMedia(rule.conditionText).matches;
          if (rule.type === CSSRule.SUPPORTS_RULE) current = current && CSS.supports(rule.conditionText);
          visit(rule.cssRules, source, conditions.concat(condition ? [condition] : []), current);
        }
      });
    }
    Array.from(document.styleSheets).forEach(function (sheet) {
      try { visit(sheet.cssRules, sheet.href ? sheet.href.split('/').slice(-2).join('/') : 'Inspector overrides', [], true); }
      catch (error) { unavailable.push(sheet.href || 'External stylesheet'); }
    });
    return { rules: result, unavailable: unavailable };
  }
  function syncEdits() {
    var node = selectedNode(); if (!selected || !node) return;
    Object.keys(selected.states).forEach(function (state) { node.toggleAttribute('data-inspect-' + state, selected.states[state]); });
    Object.keys(selected.variants).forEach(function (name) { node.classList.toggle(name, selected.variants[name]); });
    Object.keys(selected.attributes).forEach(function (name) { var value = selected.attributes[name]; if (value == null) node.removeAttribute(name); else node.setAttribute(name, value); if (name === 'checked' && 'checked' in node) node.checked = value != null; if (name === 'value' && 'value' in node) node.value = value == null ? '' : value; });
    var id = node.getAttribute('data-inspect-node');
    if (id && Object.keys(selected.css).length) {
      // CSSOM keeps user-requested style overrides out of presentation markup.
      var selector = '[data-inspect-node="' + id + '"]', rule = selected.cssRule;
      if (!rule) { overrideSheet.sheet.insertRule(selector + ' {}', overrideSheet.sheet.cssRules.length); rule = selected.cssRule = overrideSheet.sheet.cssRules[overrideSheet.sheet.cssRules.length - 1]; }
      else rule.selectorText = selector;
      Object.keys(selected.css).forEach(function (name) { if (selected.css[name]) rule.style.setProperty(name, selected.css[name], 'important'); else rule.style.removeProperty(name); });
    }
    repaint();
  }
  function section(title, open) {
    var details = element('details', open ? { open: '' } : {}); details.appendChild(element('summary', {}, title)); (sectionContainer || detail).appendChild(details); return details;
  }
  function definition(parent, name, value) { var group = element('div'); group.append(element('dt', {}, name), element('dd', {}, value)); parent.appendChild(group); }
  function defaultValue(schema) {
    if (schema.kind === 'enum') return schema.values[0];
    if (schema.kind === 'number') return 0;
    if (schema.kind === 'boolean') return false;
    if (schema.kind === 'array') return [];
    if (schema.kind === 'union') return defaultValue(schema.choices[0]);
    if (schema.kind === 'object') { var out = {}; Object.keys(schema.properties).forEach(function (key) { if (!schema.properties[key].optional) out[key] = defaultValue(schema.properties[key]); }); return out; }
    return '';
  }
  function valid(value, schema) {
    if (value === undefined) return !!schema.optional;
    if (value === null) return !!schema.nullable;
    if (schema.kind === 'union') return schema.choices.some(function (choice) { return valid(value, choice); });
    if (schema.kind === 'enum') return schema.values.indexOf(value) >= 0;
    if (schema.kind === 'number') return typeof value === 'number' && Number.isFinite(value);
    if (schema.kind === 'array') return Array.isArray(value) && value.every(function (item) { return valid(item, schema.items); });
    if (schema.kind === 'object') return typeof value === 'object' && !Array.isArray(value) && Object.keys(schema.properties).every(function (key) { return valid(value[key], schema.properties[key]); });
    if (schema.format === 'css') return !!value && typeof value === 'object' && !Array.isArray(value) && Object.keys(value).every(function (name) { return typeof value[name] === 'string' || typeof value[name] === 'number'; });
    if (schema.kind === 'string' || schema.kind === 'boolean') return typeof value === schema.kind;
    return true;
  }
  function fields(parent, schema, value, path, depth) {
    if (depth > 9) return;
    var name = path.join('.'), id = 'inspection-field-' + (++sequence), group = element('fieldset', { 'data-property': name });
    group.appendChild(element('legend', {}, name || 'Props')); parent.appendChild(group);
    if (schema.kind === 'action' || (schema.kind === 'node' && value != null && typeof value !== 'string' && typeof value !== 'number')) {
      group.appendChild(element('p', {}, schema.kind === 'action' ? 'Callback · preserved while previewing.' : 'Rendered content · select its components in the tree.')); return;
    }
    if (value == null && (schema.kind === 'object' || schema.kind === 'array')) {
      var add = element('button', { type: 'button' }, 'Set ' + (schema.kind === 'array' ? 'list' : 'object'));
      add.addEventListener('click', function () { selected.apply(path, defaultValue(schema)); refreshAfterEdit(); }); group.appendChild(add);
    } else if (schema.kind === 'object') {
      var children = group;
      function buildFields() { Object.keys(schema.properties).forEach(function (key) { fields(children, schema.properties[key], value && value[key], path.concat(key), depth + 1); }); }
      if (depth > 1) {
        children = element('details'); children.appendChild(element('summary', {}, 'Expand ' + Object.keys(schema.properties).length + ' attributes')); group.appendChild(children);
        var built = false; children.addEventListener('toggle', function () { if (children.open && !built) { built = true; buildFields(); } });
      } else buildFields();
    } else if (schema.kind === 'array') {
      (value || []).forEach(function (item, index) {
        var container = element('details', index === 0 ? { open: '' } : {}); container.appendChild(element('summary', {}, '[' + index + ']')); group.appendChild(container);
        fields(container, schema.items, item, path.concat(index), depth + 1);
        var remove = element('button', { type: 'button' }, 'Remove item ' + index);
        remove.addEventListener('click', function () { selected.apply(path, value.filter(function (_, position) { return index !== position; })); refreshAfterEdit(); }); container.appendChild(remove);
      });
      var append = element('button', { type: 'button' }, 'Add item');
      append.addEventListener('click', function () { selected.apply(path, (value || []).concat([value && value.length ? serializable(value[0]) : defaultValue(schema.items)])); refreshAfterEdit(); }); group.appendChild(append);
    } else if (schema.kind === 'union') {
      group.appendChild(element('label', { for: id }, 'Value type'));
      var choose = element('select', { id: id });
      var active = schema.choices.findIndex(function (choice) { return valid(value, choice); });
      schema.choices.forEach(function (choice, index) { var option = element('option', { value: index }, choice.kind === 'enum' ? 'Single value' : choice.kind); choose.appendChild(option); });
      choose.value = active < 0 ? 0 : active; group.appendChild(choose);
      choose.addEventListener('change', function () { selected.apply(path, defaultValue(schema.choices[Number(choose.value)])); refreshAfterEdit(); });
      fields(group, schema.choices[active < 0 ? 0 : active], value, path, depth + 1);
    } else {
      group.appendChild(element('label', { for: id }, 'Value' + (schema.optional ? ' (optional)' : '')));
      var kind = schema.kind === 'runtime' ? typeof value === 'boolean' ? 'boolean' : typeof value === 'number' ? 'number' : typeof value === 'object' ? 'json' : 'string' : schema.kind;
      var control;
      if (kind === 'enum' || kind === 'boolean') {
        control = element('select', { id: id });
        control.appendChild(element('option', Object.assign({ value: '' }, !schema.optional && value !== undefined ? { disabled: '' } : {}), 'Default (unset)'));
        var values = kind === 'enum' ? schema.values : [true, false];
        values.forEach(function (choice) { control.appendChild(element('option', { value: JSON.stringify(choice) }, String(choice))); });
        if (schema.nullable) control.appendChild(element('option', { value: 'null' }, 'null'));
        control.value = value === undefined ? '' : JSON.stringify(value);
      } else if (kind === 'json') { control = element('textarea', { id: id, rows: 4, spellcheck: 'false' }); control.value = value === undefined ? '' : JSON.stringify(serializable(value), null, 2); }
      else { control = element('input', { id: id, type: kind === 'number' ? 'number' : 'text', placeholder: 'Default (unset)' }); if (kind === 'number') control.step = 'any'; control.value = value == null ? '' : String(value); }
      if (schema.format === 'icon') control.setAttribute('list', 'inspection-icons');
      group.appendChild(control);
      control.addEventListener('change', function () {
        var next;
        try {
          if ((kind === 'enum' || kind === 'boolean') && control.value === '') selected.apply(path, undefined, true);
          else {
            if (kind === 'enum' || kind === 'boolean' || kind === 'json') next = JSON.parse(control.value);
            else if (kind === 'number') { if (!control.value.trim()) throw new Error('Enter a finite number, or use Default.'); next = Number(control.value); if (!Number.isFinite(next)) throw new Error('Enter a finite number.'); }
            else next = control.value;
            if (!valid(next, schema)) throw new Error('Value does not match ' + (schema.type || schema.kind) + '.');
            selected.apply(path, next);
          }
          control.setCustomValidity(''); say(name + ' updated.'); refreshAfterEdit(true);
        } catch (error) { control.setCustomValidity(error.message); control.reportValidity(); say(error.message); }
      });
    }
    if (schema.type) group.appendChild(element('small', {}, schema.type));
    if (path.length) {
      var reset = element('button', { type: 'button' }, 'Default');
      reset.addEventListener('click', function () { selected.apply(path, at(selected.source, path), at(selected.source, path) === undefined); refreshAfterEdit(); }); group.appendChild(reset);
      if (schema.optional) { var unset = element('button', { type: 'button' }, 'Unset'); unset.addEventListener('click', function () { selected.apply(path, undefined, true); refreshAfterEdit(); }); group.appendChild(unset); }
      if (schema.nullable) { var nullButton = element('button', { type: 'button' }, 'Set null'); nullButton.addEventListener('click', function () { selected.apply(path, null); refreshAfterEdit(); }); group.appendChild(nullButton); }
    }
  }
  function refreshAfterEdit(preserveFields) {
    var active = document.activeElement, field = active && active.closest('[data-property]'), path = field && field.getAttribute('data-property');
    requestAnimationFrame(function () {
      if (!selected) return;
      target = roots(selected)[0] || null; renderDetail(preserveFields); syncEdits();
      if (!preserveFields && path != null) {
        var groups = detail.querySelectorAll('[data-property]');
        Array.from(groups).reverse().some(function (group) {
          if (group.getAttribute('data-property') !== path) return false;
          var input = group.querySelector('input, select, textarea'); if (input) input.focus({ preventScroll: true }); return !!input;
        });
      }
    });
  }

  function renderDetail(preserveFields) {
    sectionContainer = null;
    if (!preserveFields) detail.replaceChildren(); if (!selected) { detail.appendChild(element('p', {}, 'Pick a component in the preview or component list.')); return; }
    target = selectedNode();
    if (!preserveFields) {
    detail.appendChild(element('h3', {}, selected.name));
    detail.appendChild(element('p', { id: 'inspection-bounds' }));
    if (selected.error) detail.appendChild(element('p', { role: 'alert' }, selected.error));
    var props = section('Component attributes and state', true); props.setAttribute('data-inspection-props', '');
    var filter = element('input', { type: 'search', id: 'inspection-property-filter', placeholder: 'view.state, variant, health…' });
    props.append(element('label', { for: 'inspection-property-filter' }, 'Filter component attributes'), filter);
    filter.addEventListener('input', function () {
      var query = filter.value.trim().toLowerCase();
      Array.from(props.querySelectorAll('[data-property]')).reverse().forEach(function (field) { field.hidden = !!query && field.getAttribute('data-property').toLowerCase().indexOf(query) < 0 && !Array.from(field.querySelectorAll('[data-property]')).some(function (child) { return !child.hidden; }); });
    });
    var schema = metadata.components[selected.name] || { kind: 'object', properties: {} };
    schema = Object.assign({}, schema, { properties: Object.assign({}, schema.properties) });
    Object.keys(selected.props).forEach(function (name) {
      if (schema.properties[name]) return;
      var value = selected.props[name];
      schema.properties[name] = { kind: typeof value === 'function' ? 'action' : R.isValidElement(value) ? 'node' : 'runtime', optional: true, type: 'Runtime property' };
    });
    fields(props, schema, selected.props, [], 0);
    }
    var previousSnapshot = detail.querySelector('#inspection-snapshot'); if (previousSnapshot) previousSnapshot.remove();
    var snapshot = element('div', { id: 'inspection-snapshot' }); detail.appendChild(snapshot); sectionContainer = snapshot;
    var node = selectedNode(); if (!node) { sectionContainer = null; return; }
    var visual = section('Visual states and CSS variants', true), states = element('fieldset');
    states.appendChild(element('legend', {}, 'Force a visual state')); visual.appendChild(states);
    ['hover', 'active', 'focus', 'focus-visible', 'focus-within'].forEach(function (state) {
      var id = 'inspection-state-' + state, label = element('label', { for: id });
      var input = element('input', { id: id, type: 'checkbox' }); input.checked = !!selected.states[state];
      input.addEventListener('change', function () { selected.states[state] = input.checked; syncEdits(); }); label.append(input, document.createTextNode(state)); states.appendChild(label);
    });
    var variants = new Set(), bases = Array.from(node.classList).filter(function (name) { return name.indexOf('vb-') === 0 && name.indexOf('__') < 0; }).map(function (name) { return name.split('--')[0]; });
    function collect(rules) { Array.from(rules || []).forEach(function (rule) { if (rule.selectorText) (rule.selectorText.match(/\.([a-z][\w-]*--[\w-]+)/g) || []).forEach(function (name) { name = name.slice(1); if (bases.indexOf(name.split('--')[0]) >= 0) variants.add(name); }); if (rule.cssRules) collect(rule.cssRules); }); }
    Array.from(document.styleSheets).forEach(function (sheet) { try { collect(sheet.cssRules); } catch (error) {} });
    if (variants.size) {
      var choices = element('fieldset'); choices.appendChild(element('legend', {}, 'CSS modifier classes')); visual.appendChild(choices);
      Array.from(variants).sort().forEach(function (name) { var id = 'inspection-variant-' + (++sequence), label = element('label', { for: id }), input = element('input', { type: 'checkbox', id: id }); input.checked = node.classList.contains(name);
        input.addEventListener('change', function () { selected.variants[name] = input.checked; syncEdits(); }); label.append(input, document.createTextNode(name)); choices.appendChild(label); });
    }
    var attributes = section('Rendered HTML attributes'), list = element('dl'); attributes.appendChild(list);
    Array.from(node.attributes).filter(function (attribute) { return attribute.name.indexOf('data-inspect-') !== 0; }).forEach(function (attribute) { definition(list, attribute.name, attribute.value); });
    var htmlAttributes = element('form'), label = element('label', { for: 'inspection-attribute-name' }, 'Preview HTML attribute');
    var nameInput = element('input', { id: 'inspection-attribute-name', placeholder: 'aria-label, title, disabled…', required: '' });
    var valueInput = element('input', { id: 'inspection-attribute-value', placeholder: 'Value (empty sets a boolean attribute)' });
    var removeAttribute = element('input', { type: 'checkbox', id: 'inspection-remove-attribute' }), removeLabel = element('label', { for: 'inspection-remove-attribute' }); removeLabel.append(removeAttribute, document.createTextNode('Remove attribute'));
    htmlAttributes.append(label, nameInput, element('label', { for: 'inspection-attribute-value' }, 'Attribute value'), valueInput, removeLabel, element('button', { type: 'submit' }, 'Apply attribute'));
    htmlAttributes.addEventListener('submit', function (event) { event.preventDefault(); var name = nameInput.value.trim().toLowerCase();
      if (!/^(?:aria-[\w-]+|data-(?!inspect-)[\w-]+|class|title|role|disabled|checked|value|placeholder|tabindex|min|max|step|type)$/.test(name)) { nameInput.setCustomValidity('Use a data, ARIA, class, title, role or native control attribute.'); nameInput.reportValidity(); return; }
      nameInput.setCustomValidity(''); selected.attributes[name] = removeAttribute.checked ? null : valueInput.value; syncEdits(); say(name + ' applied to the rendered element.');
    }); attributes.appendChild(htmlAttributes);
    var definitions = section('Style definitions'), matched = cssRules(node);
    if (node.getAttribute('style')) definitions.appendChild(element('pre', { tabindex: '0', role: 'region', 'aria-label': 'Inline component styles' }, 'element.style { ' + node.getAttribute('style') + ' }'));
    matched.rules.forEach(function (rule) { var entry = element('details'); entry.appendChild(element('summary', {}, rule.selector + (rule.active ? '' : ' (inactive condition)'))); entry.appendChild(element('p', {}, rule.source + (rule.context ? ' · ' + rule.context : ''))); entry.appendChild(element('pre', { tabindex: '0', role: 'region', 'aria-label': 'CSS definition' }, rule.selector + ' {\n  ' + rule.css + '\n}')); definitions.appendChild(entry); });
    matched.unavailable.forEach(function (source) { definitions.appendChild(element('p', {}, 'Browser access unavailable: ' + source)); });
    var computed = section('Computed styles and custom properties'), filterId = 'inspection-style-filter';
    computed.appendChild(element('label', { for: filterId }, 'Filter computed styles'));
    var filter = element('input', { id: filterId, type: 'search', placeholder: 'color, padding, --vb-…' }), values = element('dl'); computed.append(filter, values);
    function drawComputed() {
      values.replaceChildren(); var style = getComputedStyle(node), query = filter.value.trim().toLowerCase();
      var properties = query ? Array.from(style).filter(function (name) { return name.toLowerCase().indexOf(query) >= 0; }) : computedProperties.concat(Array.from(style).filter(function (name) { return name.indexOf('--vb-') === 0; }));
      properties.forEach(function (name) { definition(values, name, style.getPropertyValue(name)); });
    }
    filter.addEventListener('input', drawComputed); drawComputed();
    var overrides = section('Preview CSS custom properties'), style = getComputedStyle(node);
    Array.from(style).filter(function (name) { return name.indexOf('--vb-') === 0; }).sort().forEach(function (name) {
      var id = 'inspection-css-' + (++sequence), input = element('input', { id: id }); input.value = style.getPropertyValue(name).trim();
      overrides.append(element('label', { for: id }, name), input);
      input.addEventListener('change', function () { selected.css[name] = input.value.trim(); syncEdits(); say(name + ' preview updated.'); });
    });
    sectionContainer = null;
  }
  function select(record) { selected = record || null; target = selected ? roots(selected)[0] || null : null; if (selected) tree.value = selected.id; renderDetail(); if (selected) say('Selected ' + selected.name + '.'); }
  function enable(preview, button) {
    if (enabled && scope === preview) { disable(); return; }
    disable(); scope = preview; enabled = true; panel.hidden = false; overlay.removeAttribute('hidden');
    preview.button = button; button.setAttribute('aria-pressed', 'true'); renderTree(); frame();
    say('Inspector enabled. Pick a component on the canvas or from the list. Escape stops inspection.');
  }
  function disable() {
    enabled = false; hovered = null; cancelAnimationFrame(boundsFrame); cancelAnimationFrame(treeFrame); treeFrame = 0;
    if (scope && scope.button) scope.button.setAttribute('aria-pressed', 'false');
    if (panel) panel.hidden = true; if (overlay) overlay.setAttribute('hidden', '');
  }
  function bindPanel() {
    if (!document.getElementById('preview-inspector') || document.getElementById('preview-inspector') === panel) return;
    panel = document.getElementById('preview-inspector'); tree = document.getElementById('inspection-tree'); detail = document.getElementById('inspection-detail'); overlay = document.getElementById('inspection-overlay'); feedback = document.getElementById('inspection-status');
    tree.addEventListener('change', function () { select(records.get(tree.value)); });
    document.getElementById('inspection-done').addEventListener('click', function () { var button = scope && scope.button; disable(); if (button) button.focus(); });
    document.getElementById('inspection-show').addEventListener('click', function () { var node = selectedNode(); if (node) node.scrollIntoView({ block: 'center', inline: 'nearest' }); });
    document.getElementById('inspection-refresh').addEventListener('click', function () { if (selected) { syncEdits(); renderDetail(); say('Inspector refreshed.'); } });
    document.getElementById('inspection-reset').addEventListener('click', function () { if (selected) { if (selected.cssRule) { Array.from(overrideSheet.sheet.cssRules).forEach(function (rule, index) { if (rule === selected.cssRule) overrideSheet.sheet.deleteRule(index); }); selected.cssRule = null; } selected.reset(); refreshAfterEdit(); say('Component reset to its example values.'); } });
    document.getElementById('inspection-copy').addEventListener('click', function () { if (selected && navigator.clipboard) navigator.clipboard.writeText(JSON.stringify(serializable(selected.props), null, 2)).then(function () { say('Component props copied.'); }, function () { say('Clipboard unavailable.'); }); });
    var icons = document.getElementById('inspection-icons');
    (window.Veilbreak.VbIcon.names || []).forEach(function (name) { icons.appendChild(element('option', { value: name })); });
  }
  function ready() {
    metadata = JSON.parse(document.getElementById('inspection-data').textContent);
    overrideSheet = element('style', { 'data-inspector-overrides': '' }); document.head.appendChild(overrideSheet);
    bindPanel();
    document.addEventListener('keydown', function (event) {
      if (!enabled || document.querySelector('#navigation-drawer[open]')) return;
      if (event.key === 'Escape') { event.preventDefault(); event.stopPropagation(); var button = scope && scope.button; disable(); if (button) button.focus(); }
      else if (event.key === 'Enter' || event.key === ' ') { var record = pick(event.target); if (record) { event.preventDefault(); event.stopPropagation(); select(record); } }
    }, true);
    document.addEventListener('pointermove', function (event) { if (enabled) hovered = pick(event.target); }, true);
    ['pointerdown', 'pointerup', 'click', 'dblclick', 'pointerover', 'pointerout'].forEach(function (type) {
      document.addEventListener(type, function (event) {
        if (!enabled) return; var record = pick(event.target); if (!record) return;
        event.preventDefault(); event.stopPropagation(); if ((type === 'pointerdown' || type === 'click') && selected !== record) select(record);
      }, true);
    });
  }

  function attach(host, entry) {
    if (metadata) bindPanel();
    var preview = { host: host, entry: entry }, button = element('button', { type: 'button', 'aria-pressed': 'false', 'aria-controls': 'preview-inspector' }, 'Inspect');
    var caption = host.parentElement.querySelector('figcaption');
    if (caption) caption.appendChild(button); else { var controls = element('p', { 'data-preview-controls': '' }); controls.appendChild(button); host.before(controls); }
    button.addEventListener('click', function () { selected = null; target = null; enable(preview, button); });
    preview.button = button; return preview;
  }
  function detach() { disable(); scope = null; selected = null; target = null; if (overrideSheet) overrideSheet.textContent = ''; }
  window.VeilbreakInspector = { install: install, createElement: factory, wrap: function (node, preview) { return create(Scope.Provider, { value: preview }, create(Scene, { preview: preview, node: node })); }, attach: attach, detach: detach };
  document.addEventListener('DOMContentLoaded', ready);
})();
