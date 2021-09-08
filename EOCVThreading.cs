using ICities;

namespace EnhancedOutsideConnectionsView
{
    /// <summary>
    /// handle threading
    /// </summary>
    public class EOCVThreading : ThreadingExtensionBase
    {
        private bool _gameDateInitialized;

        /// <summary>
        /// called once when a game is loaded
        /// </summary>
        public override void OnCreated(IThreading threading)
        {
            // do base processing
            base.OnCreated(threading);

            // not initialized
            _gameDateInitialized = false;
        }

        /// <summary>
        /// called after every simulation tick, even when simulation is paused
        /// </summary>
        public override void OnAfterSimulationTick()
        {
            // do base processing
            base.OnAfterSimulationTick();

            // The following analysis was performed to determine the likelihood
            // that a snapshot could be missed on day one of a game month

            // approximate ticks per game day on minimal city (pop 100):
            // sim speed 1 = 585 x1 base game, More Simulation Speed Options, V10Speed
            // sim speed 2 = 293 x2 base game, More Simulation Speed Options, V10Speed
            // sim speed 3 = 145 x4 base game, More Simulation Speed Options, V10Speed
            // sim speed   =  98 x6            More Simulation Speed Options
            // sim speed   =  72 x8                                           V10Speed
            // sim speed   =  65 x9            More Simulation Speed Options, V10Speed
            // sim speed   =  37 x16                                          V10Speed
            // sim speed   =  18 x32                                          V10Speed
            // sim speed   =   9 x64                                          V10Speed
            // sim speed   =   5 x128                                         V10Speed
            // sim speed   = 2-3 x256                                         V10Speed
            // sim speed   = 1-2 x512                                         V10Speed

            // Speed Slider V2 mod does not cause ticks per game day to change
            // even when Speed Slider V2 is used with the other speed mods

            // Game Speed mod and Real Time mod can only make the game run slower (i.e. more ticks per game day)
            // so there is no concern with those two mods on missing day one snapshot

            // on my PC, going past x16 on V10Speed did not make the minimal city run any faster
            // perhaps a faster PC could make use of the higher speeds on V10Speed

            // ticks per game day do not change with bigger more populated cities (pop 500000+)

            // in the worst case (V10speed at x512), there is still at least one tick per game day
            // so that the following routine will be called on day one of a game month to take a snapshot

            // when game date is initialized, process snapshots
            if (_gameDateInitialized)
            {
                EOCVSnapshots.instance.SimulationTick();
            }
        }

        /// <summary>
        /// the game date is not initialized until the first call to OnUpdate
        /// </summary>
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            // do base processing
            base.OnUpdate(realTimeDelta, simulationTimeDelta);

            // game date is initialized
            _gameDateInitialized = true;
        }
    }
}
