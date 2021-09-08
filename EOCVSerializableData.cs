using ICities;
using System;
using System.IO;

namespace EnhancedOutsideConnectionsView
{
    public class EOCVSerializableData : SerializableDataExtensionBase
    {
        private const string SerializationDataID = "EOCVResourceSnapshots";
        private const int CurrentSnapshotVersion = 1;

        /// <summary>
        /// called when a game or editor is loaded
        /// </summary>
        public override void OnLoadData()
        {
            try
            {
                // do only for game (i.e. ignore for editors)
                if (serializableDataManager.managers.loading.currentMode != AppMode.Game)
                {
                    return;
                }

                // lock thread while working with snapshots
                EOCVSnapshots.instance.LockThread();

                // initialize snapshots
                EOCVSnapshots.instance.Initialize();

                // load the data from the game file
                byte[] data = serializableDataManager.LoadData(SerializationDataID);
                if (data == null)
                {
                    // this is not an error, it just means no data was previously saved
                    EOCVSnapshots.instance.Loaded = true;
                    return;
                }

                // make sure the data contains at least the version
                if (data.Length < 4)
                {
                    LogUtil.LogError("Version is missing from the snapshot data.");
                    return;
                }

                // read the data
                using (MemoryStream ms = new MemoryStream(data))
                {
                    using (BinaryReader reader = new BinaryReader(ms))
                    {
                        // get version
                        int version = reader.ReadInt32();

                        // compute the number of data bytes after the version
                        int dataBytes = data.Length - 4;

                        // read version 1
                        if (version == 1)
                        {
                            // confirm the data bytes divide evenly into snapshots
                            // 4 bytes per int times 16 ints per snapshot
                            const int bytesPerSnapshot = 4 * 16;
                            if (dataBytes % bytesPerSnapshot != 0)
                            {
                                LogUtil.LogError($"The number of snapshot data bytes [{dataBytes}] does not divide evenly by the bytes per snapshot [{bytesPerSnapshot}].");
                                return;
                            }

                            // get the snapshots
                            int snapshotCount = dataBytes / bytesPerSnapshot;
                            for (int i = 0; i < snapshotCount; i++)
                            {
                                // build a new snapshot
                                EOCVSnapshot snapshot = new EOCVSnapshot();

                                // read one Int32 at a time to ensure they are read in the correct order

                                // snapshot date was saved as individual values
                                int year  = reader.ReadInt32();
                                int month = reader.ReadInt32();
                                int day   = reader.ReadInt32();
                                snapshot.SnapshotDate = new DateTime(year, month, day);

                                // read imports
                                snapshot.ImportGoods    = reader.ReadInt32();
                                snapshot.ImportForestry = reader.ReadInt32();
                                snapshot.ImportFarming  = reader.ReadInt32();
                                snapshot.ImportOre      = reader.ReadInt32();
                                snapshot.ImportOil      = reader.ReadInt32();
                                snapshot.ImportMail     = reader.ReadInt32();

                                // read exports
                                snapshot.ExportGoods    = reader.ReadInt32();
                                snapshot.ExportForestry = reader.ReadInt32();
                                snapshot.ExportFarming  = reader.ReadInt32();
                                snapshot.ExportOre      = reader.ReadInt32();
                                snapshot.ExportOil      = reader.ReadInt32();
                                snapshot.ExportMail     = reader.ReadInt32();
                                snapshot.ExportFish     = reader.ReadInt32();

                                // add the snapshot
                                EOCVSnapshots.instance.Add(snapshot);
                            }

                            // success, even if there were no snapshots to load
                            EOCVSnapshots.instance.Loaded = true;
                        }
                        else
                        {
                            LogUtil.LogError($"Snapshot data version [{version}] is unexpected.");
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogException(ex);
            }
            finally
            {
                // make sure thread is unlocked
                EOCVSnapshots.instance.UnlockThread();
            }
        }

        /// <summary>
        /// called when a game or editor is saved (including AutoSave)
        /// </summary>
        public override void OnSaveData()
        {
            try
            {
                // do only for game (i.e. ignore for editors)
                if (serializableDataManager.managers.loading.currentMode != AppMode.Game)
                {
                    return;
                }

                // lock thread while working with snapshots
                EOCVSnapshots.instance.LockThread();

                // construct the data to save
                byte[] data;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        // save version
                        writer.Write(CurrentSnapshotVersion);

                        // save each snapshot
                        foreach (EOCVSnapshot snapshot in EOCVSnapshots.instance)
                        {
                            // save snapshot date as individual values because BinaryWriter.Write does not support DateTime
                            writer.Write(snapshot.SnapshotDate.Year);
                            writer.Write(snapshot.SnapshotDate.Month);
                            writer.Write(snapshot.SnapshotDate.Day);

                            // save imports
                            writer.Write(snapshot.ImportGoods);
                            writer.Write(snapshot.ImportForestry);
                            writer.Write(snapshot.ImportFarming);
                            writer.Write(snapshot.ImportOre);
                            writer.Write(snapshot.ImportOil);
                            writer.Write(snapshot.ImportMail);

                            // save exports
                            writer.Write(snapshot.ExportGoods);
                            writer.Write(snapshot.ExportForestry);
                            writer.Write(snapshot.ExportFarming);
                            writer.Write(snapshot.ExportOre);
                            writer.Write(snapshot.ExportOil);
                            writer.Write(snapshot.ExportMail);
                            writer.Write(snapshot.ExportFish);
                        }
                    }

                    // convert stream to array
                    data = ms.ToArray();
                }

                // save the data to the game file
                serializableDataManager.SaveData(SerializationDataID, data);
            }
            catch (Exception ex)
            {
                LogUtil.LogException(ex);
            }
            finally
            {
                // make sure thread is unlocked
                EOCVSnapshots.instance.UnlockThread();
            }
        }
    }
}
