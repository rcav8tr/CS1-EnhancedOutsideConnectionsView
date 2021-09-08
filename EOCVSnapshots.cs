using System;
using System.Collections.Generic;
using System.Threading;

namespace EnhancedOutsideConnectionsView
{
    /// <summary>
    /// the resource snapshots
    /// </summary>
    public class EOCVSnapshots : List<EOCVSnapshot>
    {
        // use singleton pattern
        private static readonly EOCVSnapshots _instance = new EOCVSnapshots();
        public static EOCVSnapshots instance { get { return _instance; } }
        private EOCVSnapshots() { }
        
        // miscellaneous
        public bool Loaded;
        private DateTime _previousSnapshot;
        private static readonly DateTime MaxSnapshotDate = new DateTime(9999, 12, 1);
        private static readonly object _lockObject = new object();

        /// <summary>
        /// lock thread while working with snapshots
        /// </summary>
        public void LockThread()
        {
            Monitor.Enter(_lockObject);
        }

        /// <summary>
        /// unlock thread when done working with snapshots
        /// </summary>
        public void UnlockThread()
        {
            Monitor.Exit(_lockObject);
        }

        /// <summary>
        /// initialize snapshots
        /// with singleton pattern, all fields must initialized or they will contain data from the previous game
        /// </summary>
        public void Initialize()
        {
            // reset all info
            _instance.Clear();
            Loaded = false;
            _previousSnapshot = DateTime.MinValue;
        }

        /// <summary>
        /// every simulation tick, check if should take a snapshot
        /// </summary>
        public void SimulationTick()
        {
            // if snapshots were not successfully loaded, then don't take any new snapshots
            if (!Loaded)
            {
                return;
            }

            try
            {
                // lock thread while working with snapshots
                LockThread();

                // get game date without time
                DateTime gameDate = SimulationManager.instance.m_currentGameTime.Date;

                // check if there is at least one snapshot
                bool addedSnapshot = false;
                if (_instance.Count > 0)
                {
                    // create invalid snapshots to fill in any gap between the existing snapshots and the current game date
                    // if the gap was left with no snapshots, the graph would draw a straight line between the two points on either side of the gap
                    // by creating the invalid snapshots, the graph will draw no line for the gap, which is preferred
                    // this is checked every loop because the Date Changer mod can change the game date at any time

                    // check for game date before first snapshot
                    // this can happen by using the Date Changer mod
                    DateTime firstSnapshotDate = _instance[0].SnapshotDate;
                    if (gameDate < firstSnapshotDate)
                    {
                        // fill in any gap from the game date up to but not including the first snapshot date
                        // the normal logic (below) will eventually update these snapshots

                        // if game day is 1, start first day of current game month
                        // if game day is not 1, start first day of next game month
                        DateTime startDate = new DateTime(gameDate.Year, gameDate.Month, 1);
                        if (gameDate.Day != 1)
                        {
                            startDate = startDate.AddMonths(1);
                        }
                        
                        // end one month before first snapshot date
                        DateTime endDate = firstSnapshotDate.AddMonths(-1);

                        // create an invalid snapshot for each month, starting with the end date and going backwards to the start date, inserting in front of existing snapshots
                        for (DateTime date = endDate; date >= startDate; date = date.AddMonths(-1))
                        {
                            _instance.Insert(0, EOCVSnapshot.CreateInvalid(date));
                            addedSnapshot = true;

                            // if added 01/01/0001, break out of loop to avoid subtracting one more month which results in an invalid date
                            if (date == DateTime.MinValue)
                            {
                                break;
                            }
                        }
                    }

                    // check for game date after last snapshot
                    // this can happen by using the Date Changer mod or by the user disabling this mod for some time
                    DateTime lastSnapshotDate = _instance[_instance.Count - 1].SnapshotDate;
                    if (gameDate > lastSnapshotDate && lastSnapshotDate < MaxSnapshotDate)
                    {
                        // fill in any gap from after the last snapshot up to and including the game month
                        // the normal logic below will not update these snapshots

                        // start with the last snapshot date plus one month
                        DateTime startDate = lastSnapshotDate.AddMonths(1);

                        // end with first day of the current game date month
                        // if game day is 1, then the normal logic (below) will update this last invalid snapshot
                        // if game day is not 1, then an invalid snapshot is needed for this month because the normal logic will not take a snapshot for this month
                        DateTime endDate = new DateTime(gameDate.Year, gameDate.Month, 1);

                        // create an invalid snapshot for each month, adding to the end of existing snapshots
                        for (DateTime date = startDate; date <= endDate; date = date.AddMonths(1))
                        {
                            _instance.Add(EOCVSnapshot.CreateInvalid(date));
                            addedSnapshot = true;

                            // if added 12/01/9999, break out of loop to avoid adding one more month which results in an invalid date
                            if (date == MaxSnapshotDate)
                            {
                                break;
                            }
                        }
                    }
                }

                // do normal processing only on day 1 of the game month and only if game date was not previously processed
                if (gameDate.Day == 1 && gameDate != _previousSnapshot)
                {
                    // get current snapshot
                    EOCVSnapshot snapshot = EOCVUserInterface.instance.GetResourceSnapshot();

                    // check if snapshot already exists
                    // because snapshots are sorted by date, a binary search will be much faster than a linear search
                    int index = _instance.BinarySearch(snapshot);
                    if (index >= 0)
                    {
                        // snapshot exists, update it
                        EOCVSnapshot ss = _instance[index];

                        ss.ImportGoods    = snapshot.ImportGoods;
                        ss.ImportForestry = snapshot.ImportForestry;
                        ss.ImportFarming  = snapshot.ImportFarming;
                        ss.ImportOre      = snapshot.ImportOre;
                        ss.ImportOil      = snapshot.ImportOil;
                        ss.ImportMail     = snapshot.ImportMail;

                        ss.ExportGoods    = snapshot.ExportGoods;
                        ss.ExportForestry = snapshot.ExportForestry;
                        ss.ExportFarming  = snapshot.ExportFarming;
                        ss.ExportOre      = snapshot.ExportOre;
                        ss.ExportOil      = snapshot.ExportOil;
                        ss.ExportMail     = snapshot.ExportMail;
                        ss.ExportFish     = snapshot.ExportFish;
                    }
                    else
                    {
                        // snapshot not found, add it
                        // this logic should never be reached because on the first of the month,
                        // previous logic will create an invalid snapshot which then gets found and updated
                        _instance.Add(snapshot);
                    }

                    // a snapshot was updated or added
                    addedSnapshot = true;

                    // save game date that was processed to prevent processing the same game date in the next loop
                    _previousSnapshot = gameDate;
                }

                // if a snapshot was updated or added, update the history panel
                if (addedSnapshot)
                {
                    EOCVUserInterface.instance.UpdateHistoryPanel();
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogException(ex);
            }
            finally
            {
                // make sure thread is unlocked
                UnlockThread();
            }
        }

        /// <summary>
        /// deinitialize snapshots
        /// </summary>
        public void Deinitialize()
        {
            // clear the snapshots to (hopefully) reclaim memory
            _instance.Clear();
        }
    }
}
