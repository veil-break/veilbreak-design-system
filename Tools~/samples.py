#!/usr/bin/env python3
"""Write the runnable scaffolding of each sample under Samples~: assembly definition, UXML, scene and .meta files.

Samples~ is invisible to Unity until a user imports a sample, so the scene is written as YAML here: one GameObject
with a UIDocument on Runtime/Settings/VeilbreakPortrait.asset, the sample's UXML and its MonoBehaviour. GUIDs are
derived from the file path, so reruns are stable and an imported sample keeps the references.

    uv run --frozen python Tools~/samples.py           # write
    uv run --frozen python Tools~/samples.py --check   # exit 1 when a file would change
"""

import hashlib
import json
import sys
from pathlib import Path

PACKAGE = Path(__file__).resolve().parents[1]
SAMPLES = PACKAGE / 'Samples~'
PANEL = PACKAGE / 'Runtime' / 'Settings' / 'VeilbreakPortrait.asset.meta'
# name -> (MonoBehaviour script, UXML body)
ITEMS = {
    'Gallery': (
        'GuiGallery',
        '  <ui:ScrollView name="gallery" class="vb-root" />\n',
    ),
    'BattleHud': (
        'BattleHudSample',
        '',
    ),
}
UXML_ID = 9197481963319205126  # VisualTreeAsset main object in every .uxml
SCENE = """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!29 &1
OcclusionCullingSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_OcclusionBakeSettings:
    smallestOccluder: 5
    smallestHole: 0.25
    backfaceThreshold: 100
  m_SceneGUID: 00000000000000000000000000000000
  m_OcclusionCullingData: {{fileID: 0}}
--- !u!104 &2
RenderSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 10
  m_Fog: 0
  m_AmbientMode: 3
  m_AmbientSkyColor: {{r: 0.21, g: 0.16, b: 0.3, a: 1}}
  m_SkyboxMaterial: {{fileID: 0}}
--- !u!1 &100
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 101}}
  - component: {{fileID: 102}}
  - component: {{fileID: 103}}
  m_Layer: 5
  m_Name: {name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &101
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 100}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!114 &102
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 100}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 19102, guid: 0000000000000000e000000000000000, type: 0}}
  m_Name:
  m_EditorClassIdentifier: UnityEngine.dll::UnityEngine.UIElements.UIDocument
  m_PanelSettings: {{fileID: 11400000, guid: {panel}, type: 2}}
  m_ParentUI: {{fileID: 0}}
  sourceAsset: {{fileID: {uxml_id}, guid: {uxml}, type: 3}}
  m_SortingOrder: 0
  m_Position: 0
  m_WorldSpaceSizeMode: 1
  m_WorldSpaceWidth: 1920
  m_WorldSpaceHeight: 1080
  m_PivotReferenceSize: 0
  m_Pivot: 0
  m_WorldSpaceCollider: {{fileID: 0}}
--- !u!114 &103
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 100}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {script}, type: 3}}
  m_Name:
  m_EditorClassIdentifier: {assembly}::{behaviour}
  portrait: {{fileID: 0}}
--- !u!1660057539 &9223372036854775807
SceneRoots:
  m_ObjectHideFlags: 0
  m_Roots:
  - {{fileID: 101}}
"""
IMPORTERS = {
    '.cs': 'MonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n'
    '  executionOrder: 0\n  icon: {instanceID: 0}\n',
    '.asmdef': 'AssemblyDefinitionImporter:\n  externalObjects: {}\n',
    '.uxml': 'ScriptedImporter:\n  internalIDToNameTable: []\n  externalObjects: {}\n  serializedVersion: 2\n'
    '  script: {fileID: 13804, guid: 0000000000000000e000000000000000, type: 0}\n',
    '.unity': 'DefaultImporter:\n  externalObjects: {}\n',
    '.md': 'TextScriptImporter:\n  externalObjects: {}\n',
}
TAIL = '  userData: \n  assetBundleName: \n  assetBundleVariant: \n'


def guid(path):
    return hashlib.md5(('com.veilbreak.gui/' + path.relative_to(PACKAGE).as_posix()).encode()).hexdigest()


def meta(path):
    return f'fileFormatVersion: 2\nguid: {guid(path)}\n' + IMPORTERS[path.suffix] + TAIL


def outputs():
    panel = next(line.split()[1] for line in PANEL.read_text().splitlines() if line.startswith('guid:'))
    for name, (behaviour, body) in ITEMS.items():
        folder = SAMPLES / name
        assembly = f'Veilbreak.Gui.Samples.{name}'
        asmdef = folder / f'{assembly}.asmdef'
        uxml = folder / f'{name}.uxml'
        scene = folder / f'{name}.unity'
        definition = {'name': assembly, 'references': ['Veilbreak.Gui'], 'autoReferenced': False}
        yield asmdef, json.dumps(definition, indent=2) + '\n'
        yield uxml, f'<ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:vb="Veilbreak.Gui">\n{body}</ui:UXML>\n'
        yield (
            scene,
            SCENE.format(
                name=name,
                panel=panel,
                uxml=guid(uxml),
                uxml_id=UXML_ID,
                script=guid(folder / f'{behaviour}.cs'),
                assembly=assembly,
                behaviour=behaviour,
            ),
        )
        for path in [asmdef, uxml, scene, folder / f'{behaviour}.cs', folder / 'README.md']:
            yield path.with_name(path.name + '.meta'), meta(path)


def main():
    check = '--check' in sys.argv
    stale = []
    for path, text in outputs():
        if not path.exists() or path.read_text() != text:
            stale.append(path)
            if not check:
                path.write_text(text)
    for path in stale:
        print(('stale: ' if check else 'wrote: ') + str(path.relative_to(PACKAGE)))
    sys.exit(1 if check and stale else 0)


if __name__ == '__main__':
    main()
