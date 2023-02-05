MudBun - Volumetric VFX Mesh Tool

** Importing The Package & Building Executables **
When updating from older versions of MudBun, DELETE the entire MudBun folder before re-importing from a newer package.
File structures might have changed, and directly re-importing a new package on top of an existing one could cause unexpected issues.
Please don't move MudBun's root folder and leave it as is after importing.
Due to certain limitations, some resources are referenced relative to the root asset folder. Moving the MudBun folder around can break the references.

** Bugs in Unity's Package Manager **
At the time of writing, versioning and caching of asset packages in Unity's package manager are buggy. 
The local copy of MudBun can be out of date even if the package manager displays the asset's version as up to date. 
To check the actual version of the local copy of MudBun, create and select a renderer, and the version will be shown in the inspector panel. 
If the displayed version is different from what the package manager shows, then a manual deletion of local package cache is required. 
To do so, navigate to "C:\Users\(username)\AppData\Roaming\Unity\Asset Store-5.x\Long Bunny Labs" and delete the cache for MudBun.

** Click Selection **
  Later Unity versions have disabled click selection on invisible gizmos if gizmo drawing is disabled in the Unity editor.
  In this case, gizmos drawing must be enabled for click selection to work.

** Examples ***
  There are different exmaples for different render pipelines (built-in, URP, and HDRP).
  They can be imported via an option in the start window (Tools > MudBun > MudBun Start Screen).
  It's recommended to check out examples for each render pipeline.

Long Bunny Labs: 
  LongBunnyLabs@gmail.com
  http://LongBunnyLabs.com

Author: 
  Ming-Lun (Allen) Chou
  @TheAllenChou
  http://AllenChou.net

More Info on MudBun:
  http://LongBunnyLabs.com/MudBun

MudBun User Manual:
  http://LongBunnyLabs.com/MudBun-Manual

Discord Server:
  http://discord.gg/MEGuEFU

Change Log:

Version 1.4.47
 - More aggressive multiplier for ellipsoid SDF estination to mitigate false positive voxel node culling that resulted in holes in meshes.
 - Fix editor undos in play mode causing job errors.
 - Fix destroyed object access error spam in editor.
 - Fix broken default locked mesh material for URP.
 - Expose MudRenderer.NeedsCompute. Client code can use this property to refresh logic upon mesh recomputation.
 - Fix Unity's automatic color space conversion when forwarding master color to shaders (if you've changed master colors to non-white then the visuals might change a bit).
 - Fix unexpected creation of rigid bodies when convex colliders are forced but no rigid body creation is specified.
 - Fix reverse triangle winding when number of negative components of renderer is odd in mesh render modes.
 - Fix editor error regarding duplicate menu item identifiers.

Version 1.4.46
 - Fix error on unsafe code in SdfEval.cs.

Version 1.4.35
 - Add links to online scripting API documentation to Tools > MudBun menu and start screen (online documentation still WIP).
 - Fix asynchornous Sdf.EvaluateSdfAndNormal not running.
 - Signature change to CPU-based evaluation functions: SDF and normal evaluation now both output Sdf.Result values (requires potential update to user code).
 - Fix Unity FBX Exporter not properly exporting metallic and emission by moving them to UV3 and UV4 (old locked meshes need to be re-locked for the new locked mesh shader to work properly).
 - Fix missing screen space ambient occlusion in URP10+.
 - Add options to generate UVs for texture (UV1) and light map (UV2) when locking meshes in mesh renderer mode.
 - Remove GC.Alloc calls from enum-int conversions.
 - Cache material validation to improve play mode performance in editor.

Version 1.4.34 "Raycasts"
 - CPU-based computations: SDF evaluation, gradient evaluation, raycasts, snapping to surface.
 - Raycast-based click selection by default. Gizmo-based click selection is still an option on renderers.
 - Burst is now required to use raycast-based click selection & CPU-based computations.
 - MudMaterial.IntWeight is renamed to MudMaterial.TextureWeight to better reflect its use in default shaders (users can still do whatever they want with it in custom shaders).
 - Add noise modifier brush.
 - Add MUDBUN_FAST_ITERATION define in CustomBrush.cginc for even faster iteration.
 - Only fill in necessary default values for render materials (improves build time by skipping compilation of unused render material shaders).
 - Fix edge cases of voxel edges perfectly lining up with zero isosurface.
 - Fix distortion brushes only being applied to the first brush group and not subsequent brush groups.
 - New HDRP example: Raycasts

Version 1.3.33
 - Materials are now sampled at voxel centers, rather voxel corners, for more reliable/predictable results in the Dual Quads meshing mode.
 - Fix broken simple curve when all control points are colinear.
 - Remove decompressed examples accidentaly included in previous update.

Version 1.3.32
 - Add render material mode to renderer for controlling how render materials are copied.
 - Add MudRenderer.MarkRenderMaterialDirty() for manually picking up changes in render material at run-time when using the static render material mode.
 - Fix incorrect imports of HDRP 10 compatibility package on older HDRP versions.
 - Fix warning when used with Unity timeline.

Version 1.3.31
 - Add start screen. Examples are now optionally imported via the start screen.
 - Automated import of packages specific to current render pipeline and its version.
 - Fix negative splat normal shift.
 - Fix missing meshes from particle system brushes when locking while the particle emitters are paused.
 - Fix URP object normals used as world normals.

Version 1.3.30
 - Add elongation to brushes: cylinder (scale XZ), cone (scale XZ), torus (parameter), simple curve (parameter). May change existing shapes (if that happens, set cylinder/cone XZ scale or torus Y scale to 1.0).
 - Add setters to MudRenderer's master material properties.
 - Fix unconditional inclusion of bone weights when locking meshes with vertex welding.
 - Fix renderer's Always Draw Gizmos option not drawing brush gizmos.
 - Improve bone weighting of full curve brushes.

Version 1.3.29 "Claymation"
 - Add tangent outputs to shader graph nodes.
 - Add normal map support to default mesh shaders & stopmotion mesh shader.
 - Add compute mode for controlling compute frequencies.
 - New preset mesh render materials: Clay Mesh, Claymation Mesh.
 - Fix texture tiling and offsets for URP & HDRP.
 - Fix bloated bounds for intersection brushes that could unnecessarily exhaust voxel chunk budget.
 - Fix decal render mode causing Unity to hang.
 - New HDRP examples: Claymation Terrain & Water, Claymation Fire, Claymation Water Splash.

Version 1.2.28 "Ray-Traced Voxels"
 - New render mode: Ray-Traced Voxels (experimental / URP only).
 - Collider mesh asset generation.
 - Add collider surface shift option.
 - Vertex welding supported when UV generation is turned on.
 - Duplicate utility button on brushes.
 - New simple curve properties: control point radius, smooth step blend.
 - Locked standard meshes without asset generation are also optimized for rendering.
 - Remove gizmo icons to declutter scene view.
 - Move ASE nodes under MudBun category.
 - Fix broken full curve brush when away from world origin.

Version 1.1.27
 - Quick creation panel (Window > MudBun > Quick Creation Panel).
 - Allow negative splat normal shift.

Version 1.1.26
 - Fix warning on missing config file.

Version 1.1.25
 - Fix missing namespace.

Version 1.1.24 "Ray-Marched Surface"
 - New render mode: Ray-Marched Surface (experimental / URP only).
 - Add Flip X utility button to brushes.
 - Make symmetry modes work on brush groups.
 - New URP example: Ray-Marched Blobs.
 - New HDRP example: V-Shaped Flame.
 - Fix crashes from attempting to render using disposed compute buffers.

Version 1.0.23 "Symmetry"
 - Symmetry modes for solid brushes: Flip X, Mirror X, Flip Mirror X.
 - Fix occasional missing brushes.

Version 1.0.22
 - New brush operators: Cull Inside & Cull Outside.
 - Options to match splat normals & shadows to splat camera facing.
 - Expose MudRendererBase.RenderMaterialPropertyBlock.
 - Decal render mode renders box proxy instead of fully-computed mesh.
 - Fix missing pixels along mesh triangle seams in dual meshing modes.

Version 1.0.21
 - Add assembly definitions.
 - Fix shader compatibility with HDRP 10.0.0+ (see notes above).
 - Fix moving other objects (e.g. camera) in editor triggering re-compute.
 - Fix missing shader node for decal shaders.
 - Fix forcing convex collider unconditionally generating rigid bodies.
 - Fix the last-frame state of a stopped particle system not being reflected.
 - Fix potential empty names when saving mesh assets.
 - Fix cross-renderer decal interference due to undesired depth writes.

Version 1.0.20 "Decal"
 - New render mode: Decal.
 - New preset decal render materials: Decal Paint, Decal Darken, Decal Lighten.
 - Curve noise threshold core bias.
 - Curve noise twists.
 - Add option to turn on/off vertex welding upon locking mesh in mesh renderer mode.
 - Remember materials when unlocking locked meshes.
 - SDF texture generation (can be used for collision against GPU particles from Unity's VFX graphs).
 - New Built-In RP example: Decals.
 - New HDRP examples: GPU Particle Collision, Vortex.

Version 0.9.19b
 - New advanced splats option: original normal blend.
 - Add un-cached Perlin noise type to noise volumes in case resolution artifacts from Cached Perlin noise type are undesirable.
 - Tweak particle self blend pops.
 - Bump max voxel density to 100 (be careul about performance though).
 - Fix exception upon locking mesh in mesh renderer mode.

Version 0.9.18b
 - New preset mesh render materials: Alpha-Blended Mesh (for 2D mode), Outline Mesh (for 2D mode), SDF Ripple Mesh (for 2D mode).
 - Option to force convex collider without having to create rigid bodies.
 - Generated mesh is automatically welded if no UV generation is specified (takes up less memory/disc space & friendly to mesh simplification tools).
 - Fix render bounds that caused false positive camera culling.
 - Fix camera framing of empty renderers and brushes.
 - Fix particle pops as they die off.
 - Fix exception upon disabling and re-enabling renderers.

Version 0.9.17b
 - Fix GPU crash on brush groups (caused by surface shift introduced in 0.9.15b).
 - Fix gizmos space for distortion & modifier brushes.
 - Fix hard-coded render layer.

Version 0.9.16b
 - Splat orientation jitter.
 - Recursive mesh lock/unlock.
 - Locked mesh asset generation (fixes locked meshes missing in prefabs).
 - Fix splat jitter distribution.
 - Fix IL2CPP builds on Windows.

Version 0.9.15b "2D Mode"
 - 2D mode.
 - Surface Shift parameter.
 - Force Evlauate All Brushes option.
 - New Built-In RP examples: 2D & 3D Spin Puff Comparison, 2D Sci-Fi Grid, 2D SDF Visualization.
 - Improve build time by skipping compilation of unused shaders.
 - Miscellaneous optimization.

Version 0.9.14b
 - Threshold fade on noise curve.
 - New built-in RP example: Flame.
 - Fix shadows cast by camera facing splats.
 - Fix HDRP shader nodes unnecessarily requiring custom shaders to be in the same folder as built-in shaders due to relative include paths.
 - Fix renderers with animators not regenerating mesh when animated.
 - Fix lingering mesh renderers after unlocking mesh from a newly loaded scene.
 - Fix crash on network-synced destruction.

Version 0.9.13b
 - Fix alpha cutoff issues with custom splat shaders.
 - New HDRP example: Ramen.

Version 0.9.12b
 - Fix errors when building executables.

Version 0.9.11b
 - Add option to generate UV when locking mesh in mesh renderer mode (in-editor only).
 - New preset mesh render material: Stopmotion.
 - New preset splat render materials: Brush Strokes, Floater, Floof, Leaf, Stopmotion.
 - New HDRP examples: Alpaca, Coffee, Preset Render Material Gallery, Sky Island.

Version 0.9.10b
 - Fix parented renderers incorrectly culled by bad render bounds.
 - Fix crash on undoing mesh unlock if other components on the renderer depend on components removed by the unlock logic but were not initially added by the lock logic.
 - Fix unnecessary computation when mesh is locked.
 - Fix color & emission errors when locking meshes in linear color space.
 - Fix missing voxels in transformed renderers.
 - Easier click selection & better selection framing.
 - Add quick Select Renderer & Select Brush Group buttons in inspector.
 - New HDRP example: Fog Reveal.

Version 0.9.9b
 - Fix brush groups causing exceptions when renderer is disabled.
 - Fix duplicate auto-rigged lock on start on meshes already locked and auto-rigged at edit-time.
 - Fix SDF central difference for smooth normal generation.
 - Fix brush groups not working for distortion brushes.
 - Fix lock/unlock mesh button not working for multiple selected renderers.
 - Dither texture (defaulted to blue noise).
 - Jitter dither patterns using brush hashes to mitigate obstruction of objects with identical opactiy.
 - Reduce GPU memory usage by auto-smoothing.
 - New triangle noise type for noise volumes & noise along curves.
 - Nested renderers are created at parents' origins.
 - Add warning & guidline for hierarchies mixed with MudBun objects and non-MudBun objects during auto-rigging.
 - Only draw gizmos for brushes under selected renderers or with selected brushes under the same renderers.

Version 0.9.8b
 - Initial asset store release.

Version 0.8.7b
 - Smooth corners for dual contouring.
 - Still display material properties for locked procedural meshes.
 - Nested renderer blocks recursive brush scan.
 - Fix material evaluation for auto-smoothed marching cubes.
 - Random dither.

Version 0.8.6b
 - Brush groups.
 - Auto-smoothing.
 - Add defines in CustomBrush.cginc to help temporarily reduce compile time for faster iteration.
 - Fix issues with activating/deactivating brush game objects.
 - Asynchronous mesh generation for more efficient run-time mesh locking & collider generation.
 - Procedural renderable locked mesh mode.

Version 0.8.5b
 - New render mode: quad splats.
 - New meshing modes: dual quads, surface nets, dual contouring.
 - Normal quantization.
 - Smooth normal blur.
 - Splat jitter: size, color, position, and rotation.
 - Fix splat orientation pop as view direction changes.
 - Fix edit of shared material not triggering re-rendering of affected brushes.
 - Respect scene visibiltiy in editor.

Version 0.7.4b
 - Fix objects incorrectly culled from render by bad render bounds.
 - Fix normal seams under smooth mesh render mode.
 - Fish eye distortion brush.
 - New brush material properties: splat size, texture index, and blend tightness.
 - Splat texture blends.
 - Mesh texture blends.
 - New renderer materials: single-textured/multi-textured mesh/splats.
 - Collider generation.
 - Locked mesh generation.
 - Auto-rigging of generated mesh.
 - Add "no-op" operator for brushes that do nothing but act only as bones.
 - Optimize data tranfer from CPU to GPU.
 - Add usage statistics on vertices generated.
 - Automatic upgrade of default renderer materials to current render pipeline.

Version 0.6.3b
 - Fix splats not scaling with renderer.
 - Fix missing mesh shadows in built-in RP.
 - Make GPU memory usage update actively, instead of passively on GUI events.

