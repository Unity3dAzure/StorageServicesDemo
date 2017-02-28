# Azure Blob Storage demo for Unity3d
Contains a Unity 5.5 project featuring three demo scenes for Azure Blob Storage.
1. Capture PNG screenshot (image blob)
2. Record .WAV audio clip (audio blob)
3. Save and Load .unity Asset Bundles (binary blob)
   - Save/Load XML file to load Asset Bundle prefabs to scene
   - Save/Load JSON file to load Asset Bundle prefabs to scene

# Setup Azure Storage Service for Unity3d
1. Create Azure Storage Service
2. Create blob storage container with 'public' read access for the demo.
3. Enter blob storage details in Unity Editor window.

# Credits
- Sound effects from [downloadfreesound.com](http://www.downloadfreesound.com)

# Dependencies included
- [StorageServices](https://github.com/Unity3dAzure/StorageServices) for Unity

# Notes
The Storage Services library used in this demo is currently in early beta, so not all APIs are supported yet and some things may change.
Questions or tweet #Unity #Azure @deadlyfingers
