using UnityEngine;

namespace EnhancedOutsideConnectionsView
{
    /// <summary>
    /// Harmony patching for vehicle AI
    /// </summary>
    public class VehicleAIPatch
    {
        // For vehicles not available because a DLC is not installed, the vehicle AI remains in the game logic.
        // The corresponding vehicle AI patch will simply never be called because there will be no vehicles of that type.
        // Therefore, there is no need to avoid patching a vehicle AI for missing DLC.

        // All AIs below derive from VehicleAI.
        // The vehicle AIs below preceded by "Y" have a GetColor routine with logic for Outside Connections.

        // n AircraftAI                   (base class with no buildings)
        // Y    CargoPlaneAI              (unlimited:  Cargo Airport, Cargo Airport Hub)
        // n    PassengerPlaneAI          (unlimited:  Airport, International Airport)
        // n BalloonAI                    (unlimited:  Chirper Balloon Tours, Hot Air Balloon Tours)
        // n BicycleAI                    (not generated from a service building)
        // n BlimpAI                      (base class with no buildings)
        // n    PassengerBlimpAI          (unlimited:  Blimp Depot)
        // n CableCarBaseAI               (base class with no buildings)
        // n    CableCarAI                (unlimited:  Cable Car Stop, End-of-Line Cable Car Stop)
        // n CarAI                        (base class with no buildings)
        // n    AmbulanceAI               Medical Clinic, Hospital, High-Capacity Hospital, Plastic Surgery Center (CCP), Medical Center (monument)
        // n    BusAI                     Small Emergency Shelter, Large Emergency Shelter,
        // n                              (unlimited:
        // n                                  Bus Depot, Biofuel Bus Depot,
        // n                                  Intercity Bus Station, Intercity Bus Terminal,
        // n                                  Sightseeing Bus Depots,
        // n                                  Bus-Intercity Bus Hub, Metro-Intercity Bus Hub)
        // Y    CargoTruckAI              All buildings for ExtractingFacilityAI, FishFarmingAI, FishFarmAI, ProcessingFacilityAI, UniqueFactoryAI, WarehouseAI.
        // Y                              (unlimited:  zoned industrial, Cargo Train Terminal, Cargo Harbor, Cargo Hub, Cargo Airport, Cargo Airport Hub)
        // n    DisasterResponseVehicleAI Disaster Response Unit
        // n    FireTruckAI               Fire House, Fire Station, High-Capacity Fire Station, Historical Fire Station (CCP), Fire Safety Center (CCP)
        // n    GarbageTruckAI            Landfill Site, Incineration Plant, Recycling Center, Eco-Friendly Incinerator Plant (CCP), Ultimate Recycling Plant (monument)
        // n    HearseAI                  Cemetery, Crematorium, Cryopreservatory (CCP), Crematorium Memorial Park (CCP)
        // n    MaintenanceTruckAI        Road Maintenance Depot
        // n    ParkMaintenanceVehicleAI  Park Maintenance Building
        // n    PassengerCarAI            (not generated from a service building)
        // n    PoliceCarAI               Police Station, Police Headquarters, High-Capacity Police Headquarters, Prison, Historical Police Station (CCP), Police Security Center (CCP)
        // Y    PostVanAI                 Post Office, Post Sorting Facility
        // n    SnowTruckAI               Snow Dump
        // n    TaxiAI                    Taxi Depot
        // n    TrolleybusAI              Trolleybus Depot
        // n    WaterTruckAI              Pumping Service
        // n CarTrailerAI                 (the trailer for:  some CargoTruckAI, maybe others)
        // n FerryAI                      (base class with no buildings)
        // n    FishingBoatAI             Fishing Harbor, Anchovy Fishing Harbor, Salmon Fishing Harbor, Shellfish Fishing Harbor, Tuna Fishing Harbor
        // n    PassengerFerryAI          (unlimited:  Ferry Depot)
        // n HelicopterAI                 (base class with no buildings)
        // n    AmbulanceCopterAI         Medical Helicopter Depot
        // n    DisasterResponseCopterAI  Disaster Response Unit
        // n    FireCopterAI              Fire Helicopter Depot
        // n    PoliceCopterAI            Police Helicopter Depot
        // n HelicopterDanglingAI         (TBD is this for the bucket hanging under fire copters?)
        // n MeteorAI                     (for meteor strike)
        // n PassengerHelicopterAI        (unlimited:  Helicopter Depot)
        // n PrivatePlaneAI               Aviation Club
        // n RocketAI                     ChirpX Launch Site (one rocket at a time)
        // n ShipAI                       (base class with no buildings)
        // Y    CargoShipAI               (unlimited:  Cargo Harbor, Cargo Hub)
        // n    PassengerShipAI           (unlimited:  Harbor)
        // n TrainAI                      (base class with no buildings)
        // Y    CargoTrainAI              (unlimited:  Cargo Train Terminal, Cargo Hub, Cargo Airport Hub)
        // n    PassengerTrainAI          (unlimited:  All buildings with a passenger train or monorail station.)
        // n       MetroTrainAI           (unlimited:  All buildings with a metro station.)
        // n TramBaseAI                   (base class with no buildings)
        // n    TramAI                    (unlimited:  Tram Depot)
        // n VortexAI                     (TBD for tornado?)


        /// <summary>
        /// create a patch for every vehicle AI that has a GetColor method with logic for Outside Connections
        /// in the listings above, that is vehicle AIs marked with Y
        /// </summary>
        public static bool CreateGetColorPatches()
        {
            if (!CreateGetColorPatch<CargoPlaneAI>()) return false;     // derives from AircraftAI, but AircraftAI does not have its own GetColor routine
            if (!CreateGetColorPatch<CargoShipAI >()) return false;     // derives from ShipAI,     but ShipAI     does not have its own GetColor routine
            if (!CreateGetColorPatch<CargoTrainAI>()) return false;     // derives from TrainAI,    but TrainAI    does not have its own GetColor routine

            if (!CreateGetColorPatch<CargoTruckAI>()) return false;
            if (!CreateGetColorPatch<PostVanAI   >()) return false;

            // success
            return true;
        }

        /// <summary>
        /// create a patch of the GetColor method for the specified vehicle AI type
        /// </summary>
        private static bool CreateGetColorPatch<T>() where T : VehicleAI
        {
            // same routine is used for all vehicle AI types
            return HarmonyPatcher.CreatePrefixPatchVehicleAI(typeof(T), "GetColor", typeof(VehicleAIPatch), "VehicleAIGetColor");
        }

        /// <summary>
        /// return the color of the vehicle
        /// same Prefix routine is used for all vehicle AI types
        /// </summary>
        /// <returns>whether or not to do base processing</returns>
        public static bool VehicleAIGetColor(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, ref Color __result)
        {
            // do processing for this mod only for Outside Connections info view
            if (infoMode == InfoManager.InfoMode.Connections)
            {
                return EOCVUserInterface.instance.GetVehicleColor(vehicleID, ref data, ref __result);
            }

            // do base processing
            return true;
        }
    }
}
