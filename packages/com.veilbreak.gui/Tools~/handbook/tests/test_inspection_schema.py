"""Run with python3 -m unittest discover -s Tools~/handbook/tests."""

import pathlib
import sys
import unittest

HERE = pathlib.Path(__file__).resolve().parents[1]
ROOT = HERE.parent.parent
sys.path.insert(0, str(HERE))
from inspection_schema import build_schema, split_top  # noqa: E402


class InspectionSchemaTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.schema = build_schema(
            (ROOT / 'Web~' / 'dist' / 'index.d.ts').read_text(), (ROOT / 'Tools~' / 'web' / 'kit.js').read_text()
        )['components']

    def test_skill_model_exposes_all_declared_states(self):
        state = self.schema['SkillButton']['properties']['view']['properties']['state']
        self.assertEqual(state['kind'], 'enum')
        self.assertEqual(
            set(state['values']),
            {
                'ready',
                'aiming',
                'cooldown',
                'short',
                'silenced',
                'fallen',
                'casting',
                'disabled',
                'queued',
                'buffered',
                'windup',
                'invalid',
                'rejected',
                'exposed',
            },
        )

    def test_nested_arrays_preserve_model_fields_and_enums(self):
        model = self.schema['UnitCard']['properties']['view']['properties']
        skill = model['skills']['items']['properties']
        self.assertEqual(model['mode']['values'], ['auto', 'manual', 'aiming', 'fallen'])
        self.assertEqual(skill['cost']['kind'], 'number')
        self.assertEqual(skill['state']['kind'], 'enum')

    def test_optional_nullable_models_and_polymorphic_states(self):
        view = self.schema['UnitSlot']['properties']['view']
        self.assertTrue(view['nullable'])
        self.assertEqual(view['kind'], 'object')
        state = self.schema['TileComponent']['properties']['state']
        self.assertEqual([choice['kind'] for choice in state['choices']], ['enum', 'array'])

    def test_intersections_indexed_types_and_callbacks(self):
        props = self.schema['ConfirmDialogView']['properties']
        self.assertIn('style', props)
        self.assertEqual(props['onConfirm']['kind'], 'action')
        self.assertEqual(
            props['confirm']['properties']['variant']['values'],
            self.schema['VbButton']['properties']['variant']['values'],
        )

    def test_unknown_public_types_fail_instead_of_disappearing(self):
        with self.assertRaisesRegex(ValueError, 'Unsupported inspector type'):
            build_schema('export declare function Bad(props: Unknown): React.ReactElement;', 'var api = { Bad: Bad };')

    def test_splitter_keeps_nested_function_and_object_members(self):
        self.assertEqual(
            split_top("one; { two: 'a;b'; callback: () => void }; last", ';'),
            ['one', "{ two: 'a;b'; callback: () => void }", 'last'],
        )


if __name__ == '__main__':
    unittest.main()
