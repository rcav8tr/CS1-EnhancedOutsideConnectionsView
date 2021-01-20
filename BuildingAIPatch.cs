using Harmony;
using UnityEngine;
using System;
using System.Reflection;

namespace EnhancedOutsideConnectionsView
{
    /// <summary>
    /// Harmony patching for building AI
    /// </summary>
    public class BuildingAIPatch
    {
        // DLC = Down Loadable Content
        // CCP = Content Creator Pack

        // If a building is not available because a DLC/CCP is not installed, the building AI still remains in the game logic.
        // The corresponding building AI patch will simply never be called because there will be no buildings of that type.
        // Therefore, there is no need to avoid patching a building AI for missing DLC/CCP.

        // buildings introduced in DLC:
        // BG = Base Game               03/10/15
        // AD = After Dark              09/24/15 AfterDarkDLC
        // SF = Snowfall                02/18/16 SnowFallDLC
        // MD = Match Day               06/09/16 Football
        // ND = Natural Disasters       11/29/16 NaturalDisastersDLC
        // MT = Mass Transit            05/18/17 InMotionDLC
        // CO = Concerts                08/17/17 MusicFestival
        // GC = Green Cities            10/19/17 GreenCitiesDLC
        // PL = Park Life               05/24/18 ParksDLC
        // IN = Industries              10/23/18 IndustryDLC
        // CA = Campus                  05/21/19 CampusDLC
        // SH = Sunset Harbor           03/26/20 UrbanDLC

        // buildings introduced in CCP:
        // DE = Deluxe Edition          03/10/15 DeluxeDLC
        // AR = Art Deco                09/01/16 ModderPack1
        // HT = High Tech Buildings     11/29/16 ModderPack2
        // PE = Pearls From the East    03/22/17 OrientalBuildings
        // ES = European Suburbia       10/19/17 ModderPack3 - no unique buildings, new style, "80 new special residential buildings and props"
        // UC = University City         05/21/19 ModderPack4 - no unique buildings, "adds 36 low-density residential buildings, 32 low-density commercial buildings, and 15 props"
        // MC = Modern City Center      11/07/19 ModderPack5 - no unique buildings, new style, "adds 39 unique models featuring new modern commercial wall-to-wall buildings"
        // MJ = Modern Japan            03/26/20 ModderPack6


        // The building AIs below (or a base class) marked with "Y" have a GetColor routine with logic for Outside Connections.


        // zoned building AIs are derived from PrivateBuildingAI

        // n ResidentialBuildingAI          Zoned Generic Low Density BG, Zoned Generic High Density BG, Zoned Specialized Residential (Self-Sufficient Buildings GC)
        // Y CommercialBuildingAI           Zoned Generic Low Density BG, Zoned Generic High Density BG, Zoned Specialized Commercial (Tourism AD, Leisure AD, Organic and Local Produce GC)
        // Y OfficeBuildingAI               Zoned Generic Office BG, Zoned Specialized Office (IT Cluster GC)
        // Y IndustrialBuildingAI           Zoned Generic Industrial BG
        // Y IndustrialExtractorAI          Zoned Specialized Industrial (Forest BG, Farming BG, Ore BG, Oil BG)
        // Y   LivestockExtractorAI         Zoned Specialized Industrial (Farming BG)

        // the following building AIs are from the Ploppable RICO Revisited mod
        // the growable  building AIs derive from the above zoned building AIs
        // the ploppable building AIs derive from the growable building AIs
        // n PloppableRICO.GrowableResidentialAI  PloppableRICO.PloppableResidentialAI
        // Y PloppableRICO.GrowableCommercialAI   PloppableRICO.PloppableCommercialAI
        // Y PloppableRICO.GrowableOfficeAI       PloppableRICO.PloppableOfficeAI
        // Y PloppableRICO.GrowableIndustrialAI   PloppableRICO.PloppableIndustrialAI
        // Y PloppableRICO.GrowableExtractorAI    PloppableRICO.PloppableExtractorAI
        // the Ploppable RICO Revisited mod does not have building AIs corresponding to LivestockExtractorAI


        // service building AIs are derived from PlayerBuildingAI

        // n CargoStationAI                 Cargo Train Terminal BG, Cargo Airport IN, Cargo Airport Hub IN
        // n    CargoHarborAI               Cargo Harbor BG, Cargo Hub AD
        // n CemeteryAI                     Cemetery BG, Crematorium BG, Cryopreservatory HT (CCP)
        // n ChildcareAI                    Child Health Center BG
        // n DepotAI                        Taxi Depot AD (vehicle count can be deteremined, but Taxi Depot is treated like it has unlimited)
        // n DepotAI                        Bus Depot BG, Biofuel Bus Depot GC, Trolleybus Depot SH, Tram Depot SF, Ferry Depot MT, Helicopter Depot SH, Blimp Depot MT, Sightseeing Bus Depot PL
        // n    CableCarStationAI           Cable Car Stop MT, End-of-Line Cable Car Stop MT
        // n    TransportStationAI          Bus Station AD, Helicopter Stop SH, Blimp Stop MT
        // n    TransportStationAI          Intercity Bus Station SH, Intercity Bus Terminal SH, Metro Station BG, Elevated Metro Station BG, Underground Metro Station BG, 
        // n                                Train Station BG, Airport BG, Monorail Station MT, Monorail Station with Road MT, 
        // n                                Bus-Intercity Bus Hub SH (aka Transport Hub 02 A), Bus-Metro Hub SH (aka Transport Hub 05 A), Metro-Intercity Bus Hub SH (aka Transport Hub 01 A),
        // n                                Train-Metro Hub SH (aka Transport Hub 03 A), Multiplatform End Station MT, Multiplatform Train Station MT,
        // n                                International Airport AD, Metropolitan Airport SH (aka Transport Hub 04 A), Monorail-Bus Hub MT, Metro-Monorail-Train Hub MT
        // n       HarborAI                 Ferry Stop MT, Ferry Pier MT, Ferry and Bus Exchange Stop MT
        // n       HarborAI                 Harbor BG
        // n DisasterResponseBuildingAI     Disaster Response Unit ND
        // n DoomsdayVaultAI                Doomsday Vault ND (monument)
        // n EarthquakeSensorAI             Earthquake Sensor ND
        // n EldercareAI                    Eldercare BG
        // n FireStationAI                  Fire House BG, Fire Station BG
        // n FirewatchTowerAI               Firewatch Tower ND
        // Y FishFarmAI                     Fish Farm SH, Algae Farm SH, Seaweed Farm SH
        // Y FishingHarborAI                Fishing Harbor SH, Anchovy Fishing Harbor SH, Salmon Fishing Harbor SH, Shellfish Fishing Harbor SH, Tuna Fishing Harbor SH
        // n HadronColliderAI               Hadron Collider BG (monument)
        // Y HeatingPlantAI                 Boiler Station SF, Geothermal Heating Plant SF
        // n HelicopterDepotAI              Medical Helicopter Depot ND, Fire Helicopter Depot ND, Police Helicopter Depot ND
        // n HospitalAI                     Medical Laboratory HT (CCP)
        // n HospitalAI                     Medical Clinic BG, Hospital BG, General Hospital SH (CCP)
        // n    MedicalCenterAI             Medical Center BG (monument)
        // n IndustryBuildingAI             (base clase with no buildings)
        // n    AuxiliaryBuildingAI         Forestry:  IN: Forestry Workers’ Barracks, Forestry Maintenance Building
        // n                                Farming:   IN: Farm Workers’ Barracks, Farm Maintenance Building
        // n                                Ore:       IN: Ore Industry Workers’ Barracks, Ore Industry Maintenance Building
        // n                                Oil:       IN: Oil Industry Workers’ Barracks, Oil Industry Maintenance Building
        // Y    ExtractingFacilityAI        Forestry:  IN: Small Tree Plantation, Medium Tree Plantation, Large Tree Plantation, Small Tree Sapling Greenhouse, Large Tree Sapling Greenhouse
        // Y                                Farming:   IN: Small Crops Greenhouse, Medium Crops Greenhouse, Large Crops Greenhouse, Small Fruit Greenhouse, Medium Fruit Greenhouse, Large Fruit Greenhouse
        // Y                                Ore:       IN: Small Ore Mine, Medium Ore Mine, Large Ore Mine, Small Ore Mine Underground, Large Ore Mine Underground, Seabed Mining Vessel
        // Y                                Oil:       IN: Small Oil Pump, Large Oil Pump, Small Oil Drilling Rig, Large Oil Drilling Rig, Offshore Oil Drilling Platform
        // Y    ProcessingFacilityAI        Forestry:  IN: Sawmill, Biomass Pellet Plant, Engineered Wood Plant, Pulp Mill
        // Y                                Farming:   IN: Small Animal Pasture, Large Animal Pasture, Flour Mill, Cattle Shed, Milking Parlor, 
        // Y                                Ore:       IN: Ore Grinding Mill, Glass Manufacturing Plant, Rotary Kiln Plant, Fiberglass Plant
        // Y                                Oil:       IN: Oil Sludge Pyrolysis Plant, Petrochemical Plant, Waste Oil Refining Plant, Naphtha Cracker Plant
        // Y                                Fishing:   SH: Fish Factory
        // Y       UniqueFactoryAI          IN: Furniture Factory, Bakery, Industrial Steel Plant, Household Plastic Factory, Toy Factory, Printing Press, Lemonade Factory, Electronics Factory,
        // Y                                    Clothing Factory, Petroleum Refinery, Soft Paper Factory, Car Factory, Food Factory, Sneaker Factory, Modular House Factory, Shipyard
        // n LandfillSiteAI                 Landfill Site BG, Incineration Plant BG, Recycling Center GC, Waste Transfer Facility SH, Waste Processing Complex SH, Waste Disposal Unit SH (CCP)
        // n    UltimateRecyclingPlantAI    Ultimate Recycling Plant GC (monument)
        // n LibraryAI                      Public Library BG
        // n MainCampusBuildingAI           Trade School Administration Building CA, Liberal Arts Administration Building CA, University Administration Building CA
        // n MainIndustryBuildingAI         Forestry Main Building IN, Farm Main Building IN, Ore Industry Main Building IN, Oil Industry Main Building IN
        // n MaintenanceDepotAI             Road Maintenance Depot SF, Park Maintenance Building PL
        // Y MarketAI                       Fish Market SH
        // n MonumentAI                     Landmarks:          ChirpX Launch Site BG
        // n MonumentAI                     Landmarks:          Hypermarket BG, Government Offices BG, The Gherkin BG, London Eye BG, Sports Arena BG, Theatre BG, Shopping Center BG,
        // n                                                    Cathedral BG, Amsterdam Palace BG, Winter Market BG, Department Store BG, City Hall BG, Cinema BG,
        // n                                                    Panda Sanctuary PE, Oriental Pearl Tower PE, Temple Complex PE,
        // n                                                    Traffic Park MT, Boat Museum MT, Locomotive Halls MT
        // n                                Deluxe Edition:     Statue of Liberty DE, Eiffel Tower DE, Grand Central Terminal DE, Arc de Triomphe DE, Brandenburg Gate DE
        // n                                Tourism & Leisure:  Icefishing Pond AD+SF, Casino AD, Driving Range AD, Fantastic Fountain AD, Frozen Fountain AD+SF, Luxury Hotel AD, Zoo AD
        // n                                Winter Unique:      Ice Hockey Arena SF, Ski Resort SF, Snowcastle Restaurant SF, Spa Hotel SF, Sleigh Ride SF, Snowboard Arena SF, The Christmas Tree SF, Igloo Hotel SF
        // n                                Match Day:          Football Stadium MD
        // n                                Concerts:           Festival Area CO, Media Broadcast Building CO, Music Club CO, Fan Zone Park CO
        // n                                Level 1 Unique:     Statue of Industry BG, Statue of Wealth BG, Lazaret Plaza BG, Statue of Shopping BG, Plaza of the Dead BG,
        // n                                                    Meteorite Park ND, Bird and Bee Haven GC, City Arch PL
        // n                                Level 2 Unique:     Fountain of Life and Death BG, Friendly Neighborhood Park BG, Transport Tower BG, Mall of Moderation BG, Posh Mall BG,
        // n                                                    Disaster Memorial ND, Climate Research Station GC, Clock Tower PL
        // n                                Level 3 Unique:     Colossal Order Offices BG, Official Park BG, Court House BG, Grand Mall BG, Tax Office BG,
        // n                                                    Helicopter Park ND, Lungs of the City GC, Old Market Street PL
        // n                                Level 4 Unique:     Business Park BG, Grand Library BG, Observatory BG, Opera House BG, Oppression Office BG,
        // n                                                    Pyramid Of Safety ND, Floating Gardens GC, Sea Fortress PL
        // n                                Level 5 Unique:     Servicing Services Offices BG, Academic Library BG, Science Center BG, Expo Center BG, High Interest Tower BG, Aquarium BG,
        // n                                                    Sphinx Of Scenarios ND, Ziggurat Garden GC, Observation Tower PL
        // n                                Level 6 Unique:     Cathedral of Plenitude BG, Stadium BG, MAM Modern Art Museum BG, Sea-and-Sky Scraper BG, Theater of Wonders BG,
        // n                                                    Sparkly Unicorn Rainbow Park ND, Central Park GC, The Statue of Colossalus PL
        // n                                Content Creator:    Eddie Kovanago AR, Pinoa Street AR, The Majesty AR, Electric Car Factory HT, Nanotechnology Center HT, Research Center HT,
        // n                                                    Robotics Institute HT, Semiconductor Plant HT, Software Development Studio HT, Space Shuttle Launch Site HT, Television Station HT,
        // n                                                    Drive-in Restaurant MJ, Drive-in Oriental Restaurant MJ, Oriental Market MJ, Noodle Restaurant MJ, Ramen Restaurant MJ,
        // n                                                    Service Station and Restaurant MJ, Small Office Building MJ, City Office Building MJ, District Office Building MJ,
        // n                                                    Local Register Office MJ, Resort Hotel MJ, Downtown Hotel MJ, Temple MJ, High-rise Office Building MJ,
        // n                                                    Company Headquarters MJ, Office Skyscraper MJ, The Station Department Store MJ, The Rail Yard Shopping Center MJ
        // n    AnimalMonumentAI            Winter Unique:   Santa Claus' Workshop SF
        // n    ChirpwickCastleAI           Castle Of Lord Chirpwick PL (monument)
        // n    MuseumAI                    The Technology Museum CA, The Art Gallery CA, The Science Center CA
        // n    PrivateAirportAI            Aviation Club SH (Level 5 Unique)
        // n    VarsitySportsArenaAI        Aquatics Center CA, Basketball Arena CA, Track And Field Stadium CA, Baseball Park CA, American Football Stadium CA
        // n ParkAI                         Parks:  Small Park BG, Small Playground BG, Park With Trees BG, Large Playground BG, Bouncy Castle Park BG, Botanical Garden BG,
        // n                                        Dog Park BG, Carousel Park BG, Japanese Garden BG, Tropical Garden BG, Fishing Island BG, Floating Cafe BG,
        // n                                        Snowmobile Track AD+SF, Winter Fishing Pier AD+SF, Ice Hockey Rink AD+SF
        // n                                Plazas:             Plaza with Trees BG, Plaza with Picnic Tables BG, Paradox Plaza BG (special)
        // n                                Other Parks:        Basketball Court BG, Tennis Court BG
        // n                                Tourism & Leisure:  Fishing Pier AD, Fishing Tours AD, Jet Ski Rental AD, Marina AD, Restaurant Pier AD, Beach Volleyball Court AD, Riding Stable AD, Skatepark AD
        // n                                Winter Parks:       Snowman Park SF, Ice Sculpture Park SF, Sledding Hill SF, Curling Park SF, Skating Rink SF, Ski Lodge SF, Cross-Country Skiing Park SF, Firepit Park SF
        // n                                Content Creator:    Biodome HT, Vertical Farm HT
        // n    EdenProjectAI               Eden Project BG (monument)
        // n ParkBuildingAI                 Only Amusement Park and Zoo have workers.
        // n                                City Park:       PL: Park Plaza, Park Cafe #1, Park Restrooms #1, Park Info Booth #1, Park Chess Board #1, Park Pier #1, Park Pier #2
        // n                                Amusement Park:  PL: Amusement Park Plaza, Amusement Park Cafe #1, Amusement Park Souvenir Shop #1, Amusement Park Restrooms #1, Game Booth #1, Game Booth #2,
        // n                                                     Carousel, Piggy Train, Rotating Tea Cups, Swinging Boat, House Of Horrors, Bumper Cars, Drop Tower Ride, Pendulum Ride, Ferris Wheel, Rollercoaster
        // n                                Zoo:             PL: Zoo Plaza, Zoo Cafe #1, Zoo Souvenir Shop #1, Zoo Restrooms #1, Moose And Reindeer Enclosure, Bird House, Antelope Enclosure, Bison Enclosure,
        // n                                                     {Insect, Amphibian and Reptile House}, Flamingo Enclosure, Elephant Enclosure, Sealife Enclosure, Giraffe Enclosure, Monkey Palace, Rhino Enclosure, Lion Enclosure
        // n                                Nature Reserve:  PL: Campfire Site #1, Campfire Site #2, Tent #1, Tent #2, Tent #3, Viewing Deck #1, Viewing Deck #2, Tent Camping Site #1, Lean-To Shelter #1, Lean-To Shelter #2,
        // n                                                     Lookout Tower #1, Lookout Tower #2, Camping Site #1, Fishing Cabin #1, Fishing Cabin #2, Hunting Cabin #1, Hunting Cabin #2, Bouldering Site #1
        // n ParkGateAI                     City Park:       PL: Park Main Gate, Small Park Main Gate, Park Side Gate
        // n                                Amusement Park:  PL: Amusement Park Main Gate, Small Amusement Park Main Gate, Amusement Park Side Gate
        // n                                Zoo:             PL: Zoo Main Gate, Small Zoo Main Gate, Zoo Side Gate
        // n                                Nature Reserve:  PL: Nature Reserve Main Gate, Small Nature Reserve Main Gate, Nature Reserve Side Gate
        // n PoliceStationAI                Police Station BG, Police Headquarters BG, Prison AD, Intelligence Agency HT (CCP)
        // Y PostOfficeAI                   Post Office IN, Post Sorting Facility IN
        // Y PowerPlantAI                   Coal Power Plant BG, Oil Power Plant BG, Nuclear Power Plant BG, Geothermal Power Plant GC, Ocean Thermal Energy Conversion Plant GC
        // Y                                (unlimited coal/oil reserves so cannot compute storage)
        // Y    DamPowerHouseAI             Hydro Power Plant BG
        // Y    FusionPowerPlantAI          Fusion Power Plant BG (monument)
        // Y    SolarPowerPlantAI           Solar Power Plant BG, Solar Updraft Tower GC
        // Y    WindTurbineAI               Wind Turbine BG, Advanced Wind Turbine BG, Wave Power Plant HT (CCP)
        // n RadioMastAI                    Short Radio Mast ND, Tall Radio Mast ND
        // n SaunaAI                        Sauna SF, Sports Hall and Gymnasium GC, Community Pool GC, Yoga Garden GC
        // n SchoolAI                       Elementary School BG, High School BG, University BG, Community School GC, Institute of Creative Arts GC, Modern Technology Institute GC, Faculty HT (CCP)
        // n    CampusBuildingAI            Trade School:   CA: Trade School Dormitory, Trade School Study Hall, Trade School Groundskeeping, Book Club, Trade School Outdoor Study, Trade School Gymnasium, Trade School Cafeteria,
        // n                                                    Trade School Fountain, Trade School Library, IT Club, Trade School Commencement Office, Trade School Academic Statue 1, Trade School Auditorium, Trade School Laboratories,
        // n                                                    Trade School Bookstore, Trade School Media Lab, Beach Volleyball Club, Trade School Academic Statue 2
        // n                                Liberal Arts:   CA: Liberal Arts Dormitory, Liberal Arts Study Hall, Liberal Arts Groundskeeping, Drama Club, Liberal Arts Outdoor Study, Liberal Arts Gymnasium, Liberal Arts Cafeteria,
        // n                                                    Liberal Arts Fountain, Liberal Arts Library, Art Club, Liberal Arts Commencement Office, Liberal Arts Academic Statue 1, Liberal Arts Auditorium, Liberal Arts Laboratories,
        // n                                                    Liberal Arts Bookstore, Liberal Arts Media Lab, Dance Club, Liberal Arts Academic Statue 2
        // n                                University:     CA: University Dormitory, University Study Hall, University Groundskeeping, Futsal Club, University Outdoor Study, University Gymnasium, University Cafeteria
        // n                                                    University Fountain, University Library, Math Club, University Commencement Office, University Academic Statue 1, University Auditorium, University Laboratories,
        // n                                                    University Bookstore, University Media Lab, Chess Club, University Academic Statue 2
        // n       UniqueFacultyAI          Trade School:   CA: Police Academy, School of Tourism And Travel, School of Engineering
        // n                                Liberal Arts:   CA: School of Education, School of Environmental Studies, School of Economics
        // n                                University:     CA: School of Law, School of Medicine, School of Science
        // Y ShelterAI                      Small Emergency Shelter ND, Large Emergency Shelter ND
        // n SnowDumpAI                     Snow Dump SF
        // n SpaceElevatorAI                Space Elevator BG (monument)
        // n SpaceRadarAI                   Deep Space Radar ND
        // n TaxiStandAI                    Taxi Stand AD (taxis wait at a Taxi Stand for a customer, taxis are not generated by a Taxi Stand)
        // n TollBoothAI                    Two-Way Toll Booth BG, One-Way Toll Booth BG, Two-Way Large Toll Booth BG, One-Way Large Toll Booth BG
        // n TourBuildingAI                 Hot Air Balloon Tours PL
        // n TsunamiBuoyAI                  Tsunami Warning Buoy ND
        // Y WarehouseAI                    Forestry:  IN: Small Log Yard, Saw Dust Storage, Large Log Yard, Wood Chip Storage
        // Y                                Farming:   IN: Small Grain Silo, Large Grain Silo, Small Barn, Large Barn
        // Y                                Ore:       IN: Sand Storage, Ore Storage, Ore Industry Storage, Raw Mineral Storage
        // Y                                Oil:       IN: Small Crude Oil Tank Farm, Large Crude Oil Tank Farm, Crude Oil Storage Cavern, Oil Industry Storage
        // Y                                Generic:   IN: Warehouse Yard, Small Warehouse, Medium Warehouse, Large Warehouse
        // n WaterCleanerAI                 Floating Garbage Collector GC
        // n WaterFacilityAI                Water Pumping Station BG, Water Tower BG, Large Water Tower SH, Water Drain Pipe BG, Water Treatment Plant BG,
        // n                                Inland Water Treatment Plant SH, Advanced Inland Water Treatment Plant SH, Eco Water Outlet GC, Eco Water Treatment Plant GC,
        // n                                Eco Inland Water Treatment Plant SH, Eco Advanced Inland Water Treatment Plant SH, Fresh Water Outlet ND
        // n WaterFacilityAI                Tank Reservoir ND
        // n WaterFacilityAI                Pumping Service ND
        // n WeatherRadarAI                 Weather Radar ND

        /// <summary>
        /// create a patch of the GetColor method for the specified building AI type
        /// </summary>
        private static void CreateGetColorPatch(Type buildingAIType)
        {
            // get the original GetColor method
            MethodInfo original = buildingAIType.GetMethod("GetColor");
            if (original == null)
            {
                Debug.LogError($"Unable to find GetColor method for building AI type [{buildingAIType}].");
                return;
            }

            // find the Prefix method
            MethodInfo prefix = typeof(BuildingAIPatch).GetMethod("Prefix", BindingFlags.Public | BindingFlags.Static);
            if (prefix == null)
            {
                Debug.LogError($"Unable to find BuildingAIPatch.Prefix method.");
                return;
            }

            // create the patch
            EOCV.Harmony.Patch(original, new HarmonyMethod(prefix), null, null);
        }

        /// <summary>
        /// create a patch of the GetColor method for the specified building AI type
        /// </summary>
        public static void CreateGetColorPatch<T>() where T : CommonBuildingAI
        {
            CreateGetColorPatch(typeof(T));
        }

        /// <summary>
        /// create a patch of the GetColor method for the specified building AI type (specified as string)
        /// </summary>
        /// <param name="buildingAI">building AI formatted as:  Namespace.BuildingAIType</param>
        public static void CreateGetColorPatch(string buildingAI)
        {
            // loop over all the assemblies
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // loop over all the types in the assembly
                foreach (Type type in assembly.GetTypes())
                {
                    // find the specified building AI type
                    // an AI type will be defined if the mod is subscribed, even if the mod is not enabled
                    // it is okay to patch the AI if the mod is not enabled, there simply will be no buildings of that type
                    if ($"{type.Namespace}.{type.Name}" == buildingAI)
                    {
                        // check if the type derives from CommonBuildingAI
                        bool derivesFromCommonBuildingAI = false;
                        Type baseType = type.BaseType;
                        while (baseType != null)
                        {
                            if (baseType == typeof(CommonBuildingAI))
                            {
                                derivesFromCommonBuildingAI = true;
                                break;
                            }
                            baseType = baseType.BaseType;
                        }

                        // if derived from CommonBuildingAI, then patch it
                        if (derivesFromCommonBuildingAI)
                        {
                            CreateGetColorPatch(type);
                        }
                        else
                        {
                            Debug.LogError($"Building AI [{buildingAI}] does not derive from CommonBuildingAI.");
                        }

                        // either way, found it
                        return;
                    }
                }
            }

            // if got here then the building AI was not found
            // this is not an error, it just means the mod is not subscribed or the mod changed its Namespace.BuildingAIType
        }

        /// <summary>
        /// return the color of the building
        /// same Prefix routine is used for all building AI types
        /// </summary>
        /// <returns>whether or not to do base processing</returns>
        public static bool Prefix(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, ref Color __result)
        {
            // do processing for this mod only for Outside Connections info view
            if (infoMode == InfoManager.InfoMode.Connections)
            {
                return EOCVUserInterface.GetBuildingColor(buildingID, ref data, ref __result);
            }

            // do the base processing
            return true;
        }
    }
}
