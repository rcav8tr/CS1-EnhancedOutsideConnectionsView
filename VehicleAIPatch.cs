using Harmony;
using UnityEngine;
using System;
using System.Reflection;

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
        // n BalloonAI                    (unlimited:  Hot Air Balloon Tours)
        // n BicycleAI                    (not generated from a service building)
        // n BlimpAI                      (base class with no buildings)
        // n    PassengerBlimpAI          (unlimited:  Blimp Depot)
        // n CableCarBaseAI               (base class with no buildings)
        // n    CableCarAI                (unlimited:  Cable Car Stop, End-of-Line Cable Car Stop)
        // n CarAI                        (base class with no buildings)
        // n    AmbulanceAI               Medical Clinic, Hospital, Medical Center (monument)
        // n    BusAI                     Small Emergency Shelter, Large Emergency Shelter,
        // n                              (unlimited:
        // n                                  Bus Depot, Biofuel Bus Depot,
        // n                                  Intercity Bus Station, Intercity Bus Terminal, 
        // n                                  Sightseeing Bus Depots,
        // n                                  Bus-Intercity Bus Hub, Metro-Intercity Bus Hub)
        // Y    CargoTruckAI              All buildings for ExtractingFacilityAI, FishFarmingAI, FishFarmAI, ProcessingFacilityAI, UniqueFactoryAI, WarehouseAI.
        // Y                              (unlimited:  zoned industrial, Cargo Train Terminal, Cargo Harbor, Cargo Hub, Cargo Airport, Cargo Airport Hub)
        // n    DisasterResponseVehicleAI Disaster Response Unit
        // n    FireTruckAI               Fire House, Fire Station
        // n    GarbageTruckAI            Landfill Site, Incineration Plant, Recycling Center, Ultimate Recycling Plant (monument)
        // n    HearseAI                  Cemetery, Crematorium, Cryopreservatory (CCP)
        // n    MaintenanceTruckAI        Road Maintenance Depot
        // n    ParkMaintenanceVehicleAI  Park Maintenance Building
        // n    PassengerCarAI            (not generated from a service building)
        // n    PoliceCarAI               Police Station, Police Headquarters, Prison
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
        // n    PassengerTrainAI          (unlimited:  Train Station, Multiplatform End Station, Multiplatform Train Station,
        // n                                           Monorail Station, Monorail Station with Road, Monorail-Bus Hub, Metro-Monorail-Train Hub,
        // n                                           Train-Metro Hub)
        // n       MetroTrainAI           (unlimited:  Metro Station, Elevated Metro Station, Underground Metro Station,
        // n                                           Bus-Metro Hub, Metro-Intercity Bus Hub, Train-Metro Hub, International Airport, Metro-Monorail-Train Hub)
        // n TramBaseAI                   (base class with no buildings)
        // n    TramAI                    (unlimited:  Tram Depot)
        // n VortexAI                     (TBD for tornado?)

        /// <summary>
        /// create a patch of the GetColor method for the specified vehicle AI type
        /// </summary>
        /// <remarks>
        /// Cannot use HarmonyPatch attribute because all the specific vehicle AI classes have two GetColor routines:
        /// There is a GetColor routine in the derived AI classes which has Vehicle as a parameter.
        /// There is a GetColor routine in the base clase VehicleAI which has VehicleParked as a parameter.
        /// Furthermore, MakeByRefType cannot be specified in the HarmonyPatch attribute (or any attribute) to allow the patch to be created automatically.
        /// This routine manually finds the GetColor routine with Vehicle as a ref type parameter and creates the patch for it.
        /// </remarks>
        public static void CreateGetColorPatch<T>() where T : VehicleAI
        {
            // get the original GetColor method that takes ref Vehicle parameter
            Type vehicleAIType = typeof(T);
            MethodInfo original = vehicleAIType.GetMethod("GetColor", new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InfoManager.InfoMode) });
            if (original == null)
            {
                Debug.LogError($"Unable to find GetColor method for vehicle AI type {vehicleAIType}.");
                return;
            }

            // find the Prefix method
            MethodInfo prefix = typeof(VehicleAIPatch).GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public);
            if (prefix == null)
            {
                Debug.LogError($"Unable to find VehicleAIPatch.Prefix method.");
                return;
            }

            // create the patch
            EOCV.Harmony.Patch(original, new HarmonyMethod(prefix), null, null);
        }

        /// <summary>
        /// return the color of the vehicle
        /// same Prefix routine is used for all vehicle AI types
        /// </summary>
        /// <returns>whether or not to do base processing</returns>
        public static bool Prefix(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, ref Color __result)
        {
            // do processing for this mod only for Outside Connections info view
            if (infoMode == InfoManager.InfoMode.Connections)
            {
                return EOCVUserInterface.GetVehicleColor(vehicleID, ref data, ref __result);
            }

            // do base processing
            return true;
        }
    }
}
