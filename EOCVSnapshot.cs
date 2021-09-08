using System;

namespace EnhancedOutsideConnectionsView
{
    /// <summary>
    /// a snapshot in time of import/export resource values
    /// </summary>
    public class EOCVSnapshot : IComparable<EOCVSnapshot>
    {
        // date of the snapshot
        public DateTime SnapshotDate;

        // imports
        public int ImportGoods;
        public int ImportForestry;
        public int ImportFarming;
        public int ImportOre;
        public int ImportOil;
        public int ImportMail;

        // exports
        public int ExportGoods;
        public int ExportForestry;
        public int ExportFarming;
        public int ExportOre;
        public int ExportOil;
        public int ExportMail;
        public int ExportFish;

        public EOCVSnapshot() { }

        /// <summary>
        /// constructor to make it easier to create a new instance
        /// </summary>
        public EOCVSnapshot(
            DateTime snapshotDate,
            int importGoods,
            int importForestry,
            int importFarming,
            int importOre,
            int importOil,
            int importMail,
            int exportGoods,
            int exportForestry,
            int exportFarming,
            int exportOre,
            int exportOil,
            int exportMail,
            int exportFish)
        {
            // save the snapshot date
            SnapshotDate = snapshotDate;

            // save the imports
            ImportGoods    = importGoods;
            ImportForestry = importForestry;
            ImportFarming  = importFarming;
            ImportOre      = importOre;
            ImportOil      = importOil;
            ImportMail     = importMail;

            // save the exports
            ExportGoods    = exportGoods;
            ExportForestry = exportForestry;
            ExportFarming  = exportFarming;
            ExportOre      = exportOre;
            ExportOil      = exportOil;
            ExportMail     = exportMail;
            ExportFish     = exportFish;
        }

        /// <summary>
        /// compare snapshots by comparing their dates
        /// </summary>
        public int CompareTo(EOCVSnapshot snapshot)
        {
            return SnapshotDate.CompareTo(snapshot.SnapshotDate);
        }

        /// <summary>
        /// return an invalid snapshot (has all invalid resource values)
        /// </summary>
        public static EOCVSnapshot CreateInvalid(DateTime date)
        {
            return new EOCVSnapshot(date, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1);
        }
    }
}
