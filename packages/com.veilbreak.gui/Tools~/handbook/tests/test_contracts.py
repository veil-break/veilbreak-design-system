"""Cross-platform contracts: Unity inventory, public mirrors and generated motion."""

import importlib.util
import json
import pathlib
import re
import unittest

ROOT = pathlib.Path(__file__).resolve().parents[3]


class ContractTests(unittest.TestCase):
    def test_inventory_tracks_each_public_unity_type_and_base(self):
        contract = json.loads((ROOT / 'Tokens~/components.json').read_text())['types']
        listed = {item['name']: item for item in contract}
        self.assertEqual(len(listed), len(contract), 'Types must have one canonical family')
        actual = {}
        for path in (ROOT / 'Runtime').rglob('*.cs'):
            if path.name in {'Models.cs', 'Effects.cs', 'VbTokens.cs'}:
                continue
            for name, base in re.findall(
                r'public\s+(?:(?:abstract|sealed|partial|static)\s+)*class\s+(\w+)(?:\s*:\s*([\w.]+))?',
                path.read_text(),
            ):
                actual[name] = base
        self.assertEqual(set(actual), set(listed))
        for name, base in actual.items():
            self.assertEqual(listed[name]['base'], base, name)

    def test_modal_views_share_one_abstract_base(self):
        types = {x['name']: x for x in json.loads((ROOT / 'Tokens~/components.json').read_text())['types']}
        for name in ['DrawerOverlay', 'PauseOverlayView', 'ConfirmDialogView', 'GuideDialogView', 'DialogSurface']:
            self.assertEqual(types[name]['base'], 'OverlayView')
            self.assertEqual(types[name]['family'], 'overlays')
        self.assertEqual(types['OverlayView']['kind'], 'foundation')

    def test_public_mirrors_have_a_unity_contract(self):
        source = (ROOT / 'Tools~/web/kit.js').read_text().split('var api = {', 1)[1].split('};', 1)[0]
        exports = set(re.findall(r'(\w+): \w+', source))
        types = json.loads((ROOT / 'Tokens~/components.json').read_text())['types']
        for item in types:
            if item['web']:
                self.assertIn(item['name'], exports)
        for name in ['ActivePerksView', 'RevivePickView', 'AccessibilityView', 'EffectBadge', 'EffectDetails']:
            self.assertIn(name, exports)

    def test_motion_outputs_are_current(self):
        path = ROOT / 'Tools~/motion.py'
        spec = importlib.util.spec_from_file_location('motion_generator', path)
        module = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(module)
        for output, content in module.outputs():
            self.assertEqual(output.read_text(), content)

    def test_scale_conversion_preserves_both_axes(self):
        path = ROOT / 'Tools~/web/uss2css.py'
        # The converter is also a CLI; load only its pure definitions.
        source = path.read_text().split('if __name__', 1)[0]
        source = source.split('for path in sys.argv', 1)[0]
        namespace = {'__name__': 'converter_test'}
        exec(compile(source, str(path), 'exec'), namespace)
        self.assertEqual(namespace['decl']('scale', '.05 1', '.probe'), [('scale', '.05 1')])
