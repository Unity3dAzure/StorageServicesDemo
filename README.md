# Azure Blob Storage demo for Unity3d
Contains a Unity 5.5 project featuring 4 demo scenes for Azure Blob Storage.  
1. Capture PNG screenshot (image blob)  
2. Record .WAV audio clip (audio blob)  
3. Save and Load .unity Asset Bundles (binary blob)  
   - Save/Load XML file to load Asset Bundle prefabs to scene
   - Save/Load JSON file to load Asset Bundle prefabs to scene
4. List and delete blobs

# Setup Azure Storage Service for Unity3d
1. Create Azure Storage Service
2. Create 'Blob' storage container (public read access is required for this demo).
3. Enter blob storage details in Unity Editor window.

# Credits
- Sound effects from [downloadfreesound.com](http://www.downloadfreesound.com)

# Dependencies included
- [StorageServices](https://github.com/Unity3dAzure/StorageServices) for Unity
- [TSTableView](https://bitbucket.org/tacticsoft/tstableview) is used to display recyclable list of results.

# Notes
The Storage Services library used in this demo is currently in early beta, so not all APIs are supported yet and some things may change.
Questions or tweet #Unity #Azure [@deadlyfingers](http://twitter.com/deadlyfingers)
