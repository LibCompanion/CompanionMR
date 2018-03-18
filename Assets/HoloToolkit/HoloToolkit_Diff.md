## Altered files in HoloToolkit ([v1.2017.1.2](https://github.com/Microsoft/MixedRealityToolkit-Unity/releases/tag/v1.2017.1.2)):
- Cursor.cs
- GazeManager.cs
- SpatialMappingSource.cs
- SpatialMappingObserver.cs
- MeshSaver.cs
- SpatialUnderstanding.cs
- SpatialUnderstandingCustomMesh.cs
- SpatialUnderstandingSourceMesh.cs
- InteractiveMeshCursor.cs
- WorldAnchorManager.cs



### ...\Assets\HoloToolkit\Input\Scripts\Cursor\Cursor.cs
__line 393:__
```diff
+ // #Companion: Do not interpret the clicker device as a visible hand
+ if (eventData.InputSource.GetSupportedInputInfo(eventData.SourceId) != SupportedInputInfo.None)
+ {
    visibleHandsCount++;
    IsHandVisible = true;
+ }
```



### ...\Assets\HoloToolkit\Input\Scripts\Gaze\GazeManager.cs
__line 215:__
```csharp
// #Companion: Use the Companion input module to obtain the correct pointer event data
CompanionMR.InputModule inputModule = EventSystem.current.gameObject.GetComponent<CompanionMR.InputModule>();
if (inputModule != null) { UnityUIPointerEvent = inputModule.GetLastPointerEventDataPublic(-1); }
```
__line 288:__
```csharp
// #Companion: Release the current pointer event data
if (inputModule != null) { UnityUIPointerEvent = null; }
```



### ...\Assets\HoloToolkit\SpatialMapping\Scripts\SpatialMappingSource.cs
__line 100:__
```csharp
surfaceObject.Object.tag = CompanionMR.Tags.TAG_SPATIAL_MAPPING; //#Companion
```
__line 129:__
```diff
- Debug.Log(surfaceObjectsWriteable.Count);
+ //Debug.Log(surfaceObjectsWriteable.Count); //#Companion
```



### ...\Assets\HoloToolkit\SpatialMapping\Scripts\SpatialMappingObserver.cs
__line 264:__
```csharp
// #Companion: Move the origin along with the user
this.SetObserverOrigin(Camera.main.transform.position);
this.SwitchObservedVolume();
```



### ...\Assets\HoloToolkit\SpatialMapping\Scripts\RemoteMapping\MeshSaver.cs
__line 35:__
```diff
- return ApplicationData.Current.RoamingFolder.Path;
+ return ApplicationData.Current.LocalFolder.Path; //#Companion
```



### ...\Assets\HoloToolkit\SpatialUnderstanding\Scripts\SpatialUnderstanding.cs
__line 19:__
```diff
- public const float ScanSearchDistance = 8.0f;
+ public const float ScanSearchDistance = 12.0f; //#Companion
```
__line 81:__
```diff
- SpatialMappingManager.Instance.GetComponent<SpatialMappingObserver>().TimeBetweenUpdates = (scanState == ScanStates.Done) ? UpdatePeriod_AfterScanning : UpdatePeriod_DuringScanning;
+ //SpatialMappingManager.Instance.GetComponent<SpatialMappingObserver>().TimeBetweenUpdates = (scanState == ScanStates.Done) ? UpdatePeriod_AfterScanning : UpdatePeriod_DuringScanning; //#Companion
```
__line 272:__
```csharp
/**
 * #Companion: Clean up unnecessary meshes and components.
 */
public void Cleanup() {

    // Clear ShapeDetection
    if (CompanionMR.ShapeDetection.Instance != null) { CompanionMR.ShapeDetection.Instance.ClearGeometry(); }
    if (CompanionMR.ShapeDefinition.IsInitialized) { CompanionMR.ShapeDefinition.Instance.Clear(); }

    // Clear PlacementSolver
    SpatialUnderstandingDllObjectPlacement.Solver_RemoveAllObjects();
    if (CompanionMR.PlacementSolver.Instance != null) { CompanionMR.PlacementSolver.Instance.Clear(); }

    // Clear Meshes
    if (SpatialMappingManager.IsInitialized) { SpatialMappingManager.Instance.CleanupObserver(); }
    this.UnderstandingSourceMesh.Cleanup();     // cleanup mapping mesh copies
    this.UnderstandingDLL.UnpinAllObjects();    // cleanup intern meshes
    this.UnderstandingCustomMesh.Cleanup();     // cleanup understanding meshes

    // Clear DLL
    SpatialUnderstandingDll.Imports.SpatialUnderstanding_Term();
    SpatialUnderstandingDll.Imports.SpatialUnderstanding_Init();

    // Reset SpatialUnderstanding
    this.ScanState = ScanStates.None;
}

/**
 * #Companion: Destroy the mapping components and meshes.
 */
public void CleanupMeshes() {
    if (CompanionMR.ShapeDetection.Instance != null) { CompanionMR.ShapeDetection.Instance.ClearGeometry(); }
    if (CompanionMR.PlacementSolver.Instance != null) { CompanionMR.PlacementSolver.Instance.ClearGeometry(true); }
    if (SpatialMappingManager.IsInitialized) { SpatialMappingManager.Instance.CleanupObserver(); }
    Destroy(this.UnderstandingSourceMesh); // destroy mapping mesh copies
    Destroy(this.UnderstandingCustomMesh); // destroy understanding meshes
}

/**
 * #Companion: Destroy all SpatialUnderstanding components.
 */
public void CleanupUnderstanding() {
    this.CleanupMeshes();
    if (CompanionMR.PlacementSolver.Instance != null) { Destroy(CompanionMR.PlacementSolver.Instance); }
    if (CompanionMR.ShapeDetection.Instance != null) { Destroy(CompanionMR.ShapeDetection.Instance); }
    if (CompanionMR.ShapeDefinition.IsInitialized) { Destroy(CompanionMR.ShapeDefinition.Instance); }
    Destroy(this);
}

/**
 * #Companion: Destroy this GameObject.
 */
public void DestroyGameObject() {
    Destroy(this.gameObject);
}
```



### ...\Assets\HoloToolkit\SpatialUnderstanding\Scripts\SpatialUnderstandingCustomMesh.cs
__line 446:__
```csharp
/**
 * #Companion: Clean up the mesh.
 */
public void Cleanup() {
    this.meshSectors.Clear();
    base.Cleanup();
}
```



### ...\Assets\HoloToolkit\SpatialUnderstanding\Scripts\SpatialUnderstandingSourceMesh.cs
__line 168:__
```csharp
/**
 * #Companion: Clean up the mesh list.
 */
public void Cleanup() {
    this.inputMeshList.Clear();
}
```



### ...\Assets\HoloToolkit\Utilities\Scripts\Gaze\WorldAnchorManager.cs
__line 27:__
```diff
- public TextMesh AnchorDebugText;
+ public UnityEngine.UI.Text AnchorDebugText; //#Companion
```
__line 410:__
```csharp
AnchorGameObjectReferenceList.Remove(anchorId); //#Companion
```
__line 433:__
```csharp
Destroy(anchoredGameObject); //#Companion
```
