using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ServerCore.DataModel
{
    /// <summary>
    /// Team-specific wave unlock status.
    /// Per-team tracking allows running in different time zones or reusing events.
    /// </summary>
    public class UnlockWavePerTeam
    {
        // Note: No ID here. See PuzzleServerContext.OnModelCreating for the declaration of the key.

        public int WaveID { get; set; }
        /// <summary>
        /// Unlock wave
        /// </summary>
        [Required]
        public virtual UnlockWave Wave { get; set; }

        public int TeamID { get; set; }
        /// <summary>
        ///  Team to unlock puzzles for
        /// </summary>
        [Required]
        public virtual Team Team { get; set; }

        /// <summary>
        /// Time the wave was unlocked. Null if the wave has not unlocked yet.
        /// </summary>
        public DateTimeOffset? TimeUnlocked { get; set; }
    }
}
