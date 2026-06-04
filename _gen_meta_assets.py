"""
Generates .meta files for all Unity-relevant T23 files and
.asset (UdonSharpProgramAsset) files for concrete UdonSharpBehaviours.

T23 structure:
  Runtime/Script/<dir>/T23_Foo.cs   →   Runtime/ProgramAsset/<dir>/T23_Foo.asset

Run once from the repo root, then commit everything.
Usage: python _gen_meta_assets.py
"""
import os, re, uuid

BASE = os.path.dirname(os.path.abspath(__file__))

# GUID of the UdonSharpProgramAsset MonoScript (same across UdonSharp installations)
UDON_PA_GUID = "c333ccfdd0cbdbc4ca30cef2dd6e6b9b"

SCRIPT_DIR = os.path.join(BASE, "Runtime", "Script")
ASSET_DIR  = os.path.join(BASE, "Runtime", "ProgramAsset")

SKIP_DIRS  = {".git", ".github", "Sample"}
SKIP_FILES = {"source.json", ".gitignore"}

def new_guid():
    return uuid.uuid4().hex

def write_file(path, content):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", newline="\n", encoding="utf-8") as f:
        f.write(content)

def cs_meta(g):
    return (f"fileFormatVersion: 2\nguid: {g}\nMonoImporter:\n"
            f"  externalObjects: {{}}\n  serializedVersion: 2\n  defaultReferences: []\n"
            f"  executionOrder: 0\n  icon: {{instanceID: 0}}\n  userData: \n"
            f"  assetBundleName: \n  assetBundleVariant: \n")

def asmdef_meta(g):
    return (f"fileFormatVersion: 2\nguid: {g}\nAssemblyDefinitionImporter:\n"
            f"  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n")

def folder_meta(g):
    return (f"fileFormatVersion: 2\nguid: {g}\nfolderAsset: yes\nDefaultImporter:\n"
            f"  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n")

def asset_meta(g):
    return (f"fileFormatVersion: 2\nguid: {g}\nNativeFormatImporter:\n"
            f"  externalObjects: {{}}\n  mainObjectFileID: 11400000\n  userData: \n"
            f"  assetBundleName: \n  assetBundleVariant: \n")

def default_meta(g):
    return (f"fileFormatVersion: 2\nguid: {g}\nDefaultImporter:\n"
            f"  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n")

def program_asset(name, cs_guid):
    return (
        "%YAML 1.1\n"
        "%TAG !u! tag:unity3d.com,2011:\n"
        "--- !u!114 &11400000\n"
        "MonoBehaviour:\n"
        "  m_ObjectHideFlags: 0\n"
        "  m_CorrespondingSourceObject: {fileID: 0}\n"
        "  m_PrefabInstance: {fileID: 0}\n"
        "  m_PrefabAsset: {fileID: 0}\n"
        "  m_GameObject: {fileID: 0}\n"
        "  m_Enabled: 1\n"
        "  m_EditorHideFlags: 0\n"
        f"  m_Script: {{fileID: 11500000, guid: {UDON_PA_GUID}, type: 3}}\n"
        f"  m_Name: {name}\n"
        "  m_EditorClassIdentifier: \n"
        "  serializedUdonProgramAsset: {fileID: 0}\n"
        "  udonAssembly: \n"
        "  assemblyError: \n"
        f"  sourceCsScript: {{fileID: 11500000, guid: {cs_guid}, type: 3}}\n"
        "  scriptVersion: 0\n"
        "  compiledVersion: 0\n"
        "  behaviourSyncMode: 0\n"
        "  hasInteractEvent: 0\n"
        "  scriptID: 0\n"
        "  serializationData:\n"
        "    SerializedFormat: 2\n"
        "    SerializedBytes: \n"
        "    ReferencedUnityObjects: []\n"
        "    SerializedBytesString: \n"
        "    Prefab: {fileID: 0}\n"
        "    PrefabModificationsReferencedUnityObjects: []\n"
        "    PrefabModifications: []\n"
        "    SerializationNodes:\n"
        "    - Name: fieldDefinitions\n"
        "      Entry: 6\n"
        "      Data: \n"
    )

def is_concrete_t23(path):
    try:
        text = open(path, "r", encoding="utf-8-sig").read()
    except Exception:
        return False
    if text.lstrip().startswith("#if UNITY_EDITOR"):
        return False
    return (re.search(r"public\s+class\s+T23_\w+\s*:", text)
            and not re.search(r"\babstract\s+class\b", text))

# ── Pass 1: .meta for all files/folders ──────────────────────────────────────

cs_guids = {}  # rel path (fwd slash from BASE) → guid
created  = 0

for root, dirs, files in os.walk(BASE):
    dirs[:] = [d for d in sorted(dirs) if d not in SKIP_DIRS]

    rel_root = os.path.relpath(root, BASE).replace("\\", "/")
    is_root  = (rel_root == ".")

    if not is_root:
        folder_meta_path = os.path.join(os.path.dirname(root),
                                        os.path.basename(root) + ".meta")
        if not os.path.exists(folder_meta_path):
            write_file(folder_meta_path, folder_meta(new_guid()))
            print(f"  folder.meta  {os.path.relpath(folder_meta_path, BASE)}")
            created += 1

    for fname in sorted(files):
        if fname.endswith(".meta") or fname.startswith("_gen_") or fname in SKIP_FILES:
            continue

        fpath    = os.path.join(root, fname)
        rel_file = (fname if is_root else rel_root + "/" + fname)
        meta_path = fpath + ".meta"

        if os.path.exists(meta_path):
            if fname.endswith(".cs"):
                with open(meta_path, "r") as mf:
                    for line in mf:
                        if line.startswith("guid:"):
                            cs_guids[rel_file] = line.split(":")[1].strip()
                            break
            continue

        g = new_guid()

        if fname.endswith(".cs"):
            cs_guids[rel_file] = g
            write_file(meta_path, cs_meta(g))
        elif fname.endswith(".asmdef"):
            write_file(meta_path, asmdef_meta(g))
        elif fname.endswith(".asset"):
            write_file(meta_path, asset_meta(g))
        else:
            write_file(meta_path, default_meta(g))

        print(f"  file.meta    {rel_file}.meta")
        created += 1

# ── Pass 2: .asset + .meta for each concrete T23 behaviour ───────────────────

for rel_cs, cs_guid in sorted(cs_guids.items()):
    # Only process Runtime/Script/ files
    if not rel_cs.startswith("Runtime/Script/"):
        continue
    fpath = os.path.join(BASE, rel_cs.replace("/", os.sep))
    if not is_concrete_t23(fpath):
        continue

    # Mirror path: Runtime/Script/<sub>/T23_Foo.cs → Runtime/ProgramAsset/<sub>/T23_Foo.asset
    rel_asset  = "Runtime/ProgramAsset/" + rel_cs[len("Runtime/Script/"):-3] + ".asset"
    asset_path = os.path.join(BASE, rel_asset.replace("/", os.sep))
    asset_meta_path = asset_path + ".meta"
    name = os.path.splitext(os.path.basename(rel_cs))[0]

    if not os.path.exists(asset_path):
        write_file(asset_path, program_asset(name, cs_guid))
        print(f"  .asset       {rel_asset}")
        created += 1

    if not os.path.exists(asset_meta_path):
        write_file(asset_meta_path, asset_meta(new_guid()))
        print(f"  asset.meta   {rel_asset}.meta")
        created += 1

print(f"\nDone — {created} files created.")
