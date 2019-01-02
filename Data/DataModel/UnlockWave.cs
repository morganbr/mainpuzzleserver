using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ServerCore.DataModel
{
    /// <summary>
    /// Tracks unlocking a set of puzzles together
    /// </summary>
    public class UnlockWave
    {
        public UnlockWave()
        {
        }

        public UnlockWave(UnlockWave source)
        {
            throw new NotImplementedException();
            //Name = source.Name;
            //// How to get the new PreviousWave?
            //TimeAfterPreviousWave = source.TimeAfterPreviousWave;
            //// How to get the new puzzles?
        }

        /// <summary>
        /// The ID
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Friendly name of the wave
        /// </summary>
        [Required]
        public string Name { get; set; }

        [Required]
        public virtual Event Event { get; set; }

        /// <summary>
        /// A previous wave that this should unlock a certain amount of time after. May be null.
        /// </summary>
        public virtual UnlockWave PreviousWave { get; set; }

        /// <summary>
        /// The amount of time this should unlock after PreviousWave has unlocked. If PreviousWave is null, this should be too.
        /// </summary>
        public TimeSpan? TimeAfterPreviousWave { get; set; }

        /// <summary>
        /// The puzzles unlocked by this wave.
        /// </summary>
        public virtual IList<Puzzle> UnlockPuzzles { get; set; }
    }
}
