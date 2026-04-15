using UnityEngine;

namespace PlayFrame.Systems.Grid
{
    /// <summary>
    /// Audio configuration for grid interaction events.
    /// Create via: Assets -> Create -> PlayFrame -> Grid -> Audio Config
    /// </summary>
    [CreateAssetMenu(fileName = "GridAudioConfig", menuName = "PlayFrame/Grid/Audio Config")]
    public class GridAudioConfig : ScriptableObject
    {
        [Header("Tap/Select Audio")]
        [Tooltip("Sound when a piece is tapped/selected")]
        public AudioClip tapSound;

        [Tooltip("Sound when a piece is deselected")]
        public AudioClip deselectSound;

        [Header("Swap Audio")]
        [Tooltip("Sound when two pieces start swapping")]
        public AudioClip swapStartSound;

        [Tooltip("Sound when a swap results in a valid match")]
        public AudioClip swapSuccessSound;

        [Tooltip("Sound when a swap fails (no match, pieces swap back)")]
        public AudioClip swapFailSound;

        [Header("Drag & Drop Audio")]
        [Tooltip("Sound when a piece drag starts")]
        public AudioClip dragStartSound;

        [Tooltip("Sound while dragging over different cells")]
        public AudioClip dragOverCellSound;

        [Tooltip("Sound when a piece is dropped successfully")]
        public AudioClip dropSuccessSound;

        [Tooltip("Sound when a piece is dropped back to origin (invalid)")]
        public AudioClip dropFailSound;

        [Header("Chain/Draw Audio")]
        [Tooltip("Sound when adding a piece to the chain")]
        public AudioClip chainAddSound;

        [Tooltip("Sound when removing the last piece from chain (backtrack)")]
        public AudioClip chainRemoveSound;

        [Tooltip("Sound when chain is confirmed/released")]
        public AudioClip chainConfirmSound;

        [Tooltip("Sound when chain is too short (failed)")]
        public AudioClip chainFailSound;

        [Header("Multi-Select Audio")]
        [Tooltip("Sound for each individual selection in multi-select")]
        public AudioClip multiSelectSound;

        [Tooltip("Sound when target selection count is reached")]
        public AudioClip multiSelectCompleteSound;

        [Header("Match/Destroy Audio")]
        [Tooltip("Sound when pieces match")]
        public AudioClip matchSound;

        [Tooltip("Sound when pieces are destroyed")]
        public AudioClip destroySound;

        [Tooltip("Sound for combo matches (multiple matches at once)")]
        public AudioClip comboSound;

        [Header("Spawn Audio")]
        [Tooltip("Sound when new pieces spawn")]
        public AudioClip spawnSound;

        [Header("Special Audio")]
        [Tooltip("Sound for hint display")]
        public AudioClip hintSound;

        [Tooltip("Sound when no moves are available")]
        public AudioClip noMovesSound;

        [Tooltip("Sound for grid shuffle")]
        public AudioClip shuffleSound;

        [Header("Settings")]
        [Tooltip("Pitch variation range for repeated sounds (prevents monotony)")]
        [Range(0f, 0.3f)]
        public float pitchVariation = 0.05f;

        [Tooltip("Minimum interval between same sounds to prevent overlapping")]
        [Range(0f, 0.2f)]
        public float minSoundInterval = 0.05f;
    }
}
