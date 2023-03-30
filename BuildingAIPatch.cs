using UnityEngine;
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
        // AP = Airports                01/25/22 AirportDLC
        // PP = Plazas & Promenades     09/14/22 PlazasAndPromenadesDLC
        // FD = Financial Districts     12/13/22 FinancialDistrictsDLC

        // buildings introduced in CCP:
        // DE = Deluxe Edition          03/10/15 DeluxeDLC
        // AR = Art Deco                09/01/16 ModderPack1
        // HT = High Tech Buildings     11/29/16 ModderPack2
        // PE = Pearls From the East    03/22/17 OrientalBuildings
        // ES = European Suburbia       10/19/17 ModderPack3 - no unique buildings, new style, "80 new special residential buildings and props"
        // UC = University City         05/21/19 ModderPack4 - no unique buildings, "adds 36 low-density residential buildings, 32 low-density commercial buildings, and 15 props"
        // MC = Modern City Center      11/07/19 ModderPack5 - no unique buildings, new style, "adds 39 unique models featuring new modern commercial wall-to-wall buildings"
        // MJ = Modern Japan            03/26/20 ModderPack6
        // TS = Train Stations          05/21/21 ModderPack7
        // BP = Bridges & Piers         05/21/21 ModderPack8
        // MP = Map Pack                01/25/22 ModderPack9 - no unique buildings, "8 new maps"
        // VW = Vehicles of the World   01/25/22 ModderPack10 - no unique buildings, "set of 21 new vehicle assets"
        // MM = Mid-Century Modern      09/14/22 ModderPack11 - "pack of a whopping 147 residential growable buildings" + "District style for growables", "3 Hotels and 2 restaurants", "30+ additional decorations", "Car Ports"
        // SR = Seaside Resorts         09/14/22 ModderPack12 - "pack contains 29 beautiful buildings"
        // SS = Skyscrapers             11/15/22 ModderPack13 - "24 unique skyscrapers and high rises and their 20 variants" (i.e. 44 total unique buildings)
        // HK = Heart of Korea          11/15/22 ModderPack14 - "29 Growable low-density residential, 17 Growable high-density commercial, 6 service buildings, 8 unique buildings"
        // M2 = Map Pack 2              12/13/22 ModderPack15 - no unique buildings, "10 new maps"
        // SM = Shopping Malls          03/22/23 ModderPack16 - "4 unique buildings, 53 growable commercial buildings, additional props"
        // SV = Sports Venues           03/22/23 ModderPack17 - "6 large unique stadiums, 6 mid-sized unique stadiums, 10 Community sports parks"
        // AM = Africa in Miniature     03/22/23 ModderPack18 - "11 unique buildings, 2 monuments, 4 service buildings, 9 growable buildings, 2 props"



        // For building AIs that derive from other building AIs (i.e. not derived from PrivateBuildingAI or PlayerBuildingAI):
        //     If the derived building AI has its own GetColor method with logic for Outside Connections, it is patched.
        //     If the derived building AI has no GetColor method, Harmony won't allow it to be patched.
        //         But the base building AI with logic for Outside Connections is patched and that patch will handle the derived building AI.
        // Each building AI that has its own GetColor method is marked with GC below.
        // Each building AI where its GetColor method has logic for Outside Connections is additionally marked with OC below.
        // Note that the building AIs that derive directly from PrivateBuildingAI or PlayerBuildingAI all have a GetColor method.


        // zoned building AIs are derived from PrivateBuildingAI

        // ResidentialBuildingAI            GC      Zoned Generic Low Density BG, Zoned Generic High Density BG, Zoned Specialized Residential (Self-Sufficient Buildings GC, Wall-to-Wall PP)
        // CommercialBuildingAI             GC  OC  Zoned Generic Low Density BG, Zoned Generic High Density BG, Zoned Specialized Commercial (Tourism AD, Wall-to-Wall PP, Leisure AD, Organic and Local Produce GC)
        // OfficeBuildingAI                 GC  OC  Zoned Generic Office BG, Zoned Specialized Office (IT Cluster GC, Wall-to-Wall PP)
        // IndustrialBuildingAI             GC  OC  Zoned Generic Industrial BG
        // IndustrialExtractorAI            GC  OC  Zoned Specialized Industrial (Forest BG, Farming BG, Ore BG, Oil BG)
        //    LivestockExtractorAI                  Zoned Specialized Industrial (Farming BG)

        // the following building AIs are from the Ploppable RICO Revisited mod
        // the growable  building AIs derive from the above zoned building AIs
        // the ploppable building AIs derive from the growable building AIs
        // none of the growable/ploppable building AIs have a GetColor method
        // PloppableRICO.GrowableResidentialAI  PloppableRICO.PloppableResidentialAI
        // PloppableRICO.GrowableCommercialAI   PloppableRICO.PloppableCommercialAI
        // PloppableRICO.GrowableOfficeAI       PloppableRICO.PloppableOfficeAI
        // PloppableRICO.GrowableIndustrialAI   PloppableRICO.PloppableIndustrialAI
        // PloppableRICO.GrowableExtractorAI    PloppableRICO.PloppableExtractorAI
        // the Ploppable RICO Revisited mod does not have building AIs corresponding to LivestockExtractorAI


        // service building AIs are derived from PlayerBuildingAI

        // AirportBuildingAI                GC      (base class with no buildings) AP
        //    AirportAuxBuildingAI                  Control Tower AP (3 styles), Concourse Hub AP (3 styles), Small Hangar AP, Large Hangar AP,
        //                                          Budget Airport Hotel AP, Luxury Airport Hotel AP, Airline Lounge AP, Aviation Fuel Station AP,
        //                                          Small Parked Plane AP (3 variations), Medium Parked Plane AP (3 variations), Large Parked Plane AP (2 variations), Parked Cargo Plane AP (2 variations)
        //    AirportEntranceAI                     Airport Terminal AP (3 styles), Two-Story Terminal AP (3 styles), Large Terminal AP (3 styles), Cargo Airport Terminal AP
        // BankOfficeAI                     GC      Small Bank FD, Large Bank FD, Skyscraper Bank FD
        // CargoStationAI                   GC      Cargo Train Terminal BG, Cargo Airport IN, Cargo Airport Hub IN, Airport Cargo Train Station AP
        //    AirportCargoGateAI            GC      Cargo Aircraft Stand AP
        //    CargoHarborAI                         Cargo Harbor BG, Cargo Hub AD
        // CemeteryAI                       GC      Cemetery BG, Crematorium BG, Cryopreservatory HT (CCP), Crematorium Memorial Park HK (CCP)
        // ChildcareAI                      GC      Child Health Center BG
        // DepotAI                          GC      Taxi Depot AD (vehicle count can be deteremined, but Taxi Depot is treated like it has unlimited)
        // DepotAI                          GC      Bus Depot BG, Biofuel Bus Depot GC, Trolleybus Depot SH, Tram Depot SF, Ferry Depot MT, Helicopter Depot SH, Blimp Depot MT, Sightseeing Bus Depot PL
        //    CableCarStationAI                     Cable Car Stop MT, End-of-Line Cable Car Stop MT
        //    TransportStationAI                    Bus Station AD, Compact Bus Station PP, Helicopter Stop SH, Blimp Stop MT
        //    TransportStationAI                    Bus:       Intercity Bus Station SH, Intercity Bus Terminal SH,
        //                                          Metro:     Metro Station BG, Elevated Metro Station BG, Elevated Metro Station With Shops PP, Underground Metro Station BG,
        //                                                     Parallel Underground Metro Station PP, Large Underground Metro Station PP, Metro Plaza Station TS (aka H_Hub02_A),
        //                                                     Sunken Island Platform Metro Station TS, Sunken Dual Island Platform Metro Station TS, Sunken Bypass Metro Station TS,
        //                                                     Elevated Island Platform Metro Station TS, Elevated Dual Island Platform Metro Station TS, Elevated Bypass Metro Station TS
        //                                          Train:     Train Station BG, Elevated Train Station PP, Crossover Train Station Hub TS (aka H_Hub03), Old Market Station TS (aka H_Hub04),
        //                                                     Ground Island Platform Train Station TS, Ground Dual Island Platform Train Station TS, Ground Bypass Train Station TS,
        //                                                     Elevated Island Platform Train Station TS, Elevated Dual Island Platform Train Station TS, Elevated Bypass Train Station TS,
        //                                                     Historical Train Station SR (CCP)
        //                                          Air:       Airport BG
        //                                          Monorail:  Monorail Station MT, Monorail Station with Road MT,
        //                                          Hub:       Bus-Intercity Bus Hub SH (aka Transport Hub 02 A), Bus-Metro Hub SH (aka Transport Hub 05 A), Bus-Train-Tram Hub SF,
        //                                                     Metro-Intercity Bus Hub SH (aka Transport Hub 01 A), Metro-Tram Hub with Road SF, Multi-level Metro Hub BG,
        //                                                     Train-Metro Hub SH (aka Transport Hub 03 A), Glass Box Transport Hub TS (aka H_Hub01), Multiplatform End Station MT,
        //                                                     Multiplatform Train Station MT, International Airport AD, Metropolitan Airport SH (aka Transport Hub 04 A),
        //                                                     Monorail-Bus Hub MT, Monorail-Tram Hub with Road SF+MT, Metro-Monorail-Train Hub MT, Metro-Train-Monorail-Tram Hub with Road SF+MT
        //       AirportGateAI              GC      Airport Bus Station AP
        //       AirportGateAI              GC      Small Aircraft Stand AP, Medium Aircraft Stand AP, Large Aircraft Stand AP, Elevated Airport Metro Station AP, Airport Train Station AP
        //       HarborAI                           Ferry Stop MT, Ferry Pier MT, Ferry and Bus Exchange Stop MT, Ferry-Tram Hub SF+MT
        //       HarborAI                           Harbor BG, Harbor-Ferry Hub MT, Harbor-Bus Hub BG, Harbor-Bus-Monorail Hub MT
        // DisasterResponseBuildingAI       GC      Disaster Response Unit ND, Disaster Response Air Base ND
        // DoomsdayVaultAI                  GC      Doomsday Vault ND (monument)
        // EarthquakeSensorAI               GC      Earthquake Sensor ND
        // EldercareAI                      GC      Eldercare BG
        // FireStationAI                    GC      Fire House BG, Fire Station BG, High-Capacity Fire Station PP, Historical Fire Station SR (CCP), Fire Safety Center HK (CCP)
        // FirewatchTowerAI                 GC      Firewatch Tower ND
        // FishFarmAI                       GC  OC  Fish Farm SH, Algae Farm SH, Seaweed Farm SH
        // FishingHarborAI                  GC  OC  Fishing Harbor SH, Anchovy Fishing Harbor SH, Salmon Fishing Harbor SH, Shellfish Fishing Harbor SH, Tuna Fishing Harbor SH
        // HadronColliderAI                 GC      Hadron Collider BG (monument)
        // HeatingPlantAI                   GC  OC  Boiler Station SF, Geothermal Heating Plant SF
        // HelicopterDepotAI                GC      Medical Helicopter Depot ND, Higher Capacity Medical Helicopter Depot ND,
        //                                          Fire Helicopter Depot ND, Higher Capacity Fire Helicopter Depot ND,
        //                                          Police Helicopter Depot ND, Higher Capacity Police Helicopter Depot ND
        // HospitalAI                       GC      Medical Laboratory HT (CCP)
        // HospitalAI                       GC      Medical Clinic BG, Hospital BG, High-Capacity Hospital PP, General Hospital SH (CCP), Plastic Surgery Center HK (CCP)
        //    MedicalCenterAI                       Medical Center BG (monument)
        // IndustryBuildingAI               GC      (base clase with no buildings)
        //    AuxiliaryBuildingAI           GC      Forestry:  IN: Forestry Workers’ Barracks, Forestry Maintenance Building
        //                                          Farming:   IN: Farm Workers’ Barracks, Farm Maintenance Building
        //                                          Ore:       IN: Ore Industry Workers’ Barracks, Ore Industry Maintenance Building
        //                                          Oil:       IN: Oil Industry Workers’ Barracks, Oil Industry Maintenance Building
        //    ExtractingFacilityAI          GC  OC  Forestry:  IN: Small Tree Plantation, Medium Tree Plantation, Large Tree Plantation, Small Tree Sapling Greenhouse, Large Tree Sapling Greenhouse
        //                                          Farming:   IN: Small Crops Greenhouse, Medium Crops Greenhouse, Large Crops Greenhouse, Small Fruit Greenhouse, Medium Fruit Greenhouse, Large Fruit Greenhouse
        //                                          Ore:       IN: Small Ore Mine, Medium Ore Mine, Large Ore Mine, Small Ore Mine Underground, Large Ore Mine Underground, Seabed Mining Vessel
        //                                          Oil:       IN: Small Oil Pump, Large Oil Pump, Small Oil Drilling Rig, Large Oil Drilling Rig, Offshore Oil Drilling Platform
        //    ProcessingFacilityAI          GC  OC  Forestry:  IN: Sawmill, Biomass Pellet Plant, Engineered Wood Plant, Pulp Mill
        //                                          Farming:   IN: Small Animal Pasture, Large Animal Pasture, Flour Mill, Cattle Shed, Milking Parlor,
        //                                          Ore:       IN: Ore Grinding Mill, Glass Manufacturing Plant, Rotary Kiln Plant, Fiberglass Plant
        //                                          Oil:       IN: Oil Sludge Pyrolysis Plant, Petrochemical Plant, Waste Oil Refining Plant, Naphtha Cracker Plant
        //                                          Fishing:   SH: Fish Factory
        //       UniqueFactoryAI                    IN: Furniture Factory, Bakery, Industrial Steel Plant, Household Plastic Factory, Toy Factory, Printing Press, Lemonade Factory, Electronics Factory,
        //                                              Clothing Factory, Petroleum Refinery, Soft Paper Factory, Car Factory, Food Factory, Sneaker Factory, Modular House Factory, Shipyard
        // LandfillSiteAI                   GC  xx  Landfill Site BG, Incineration Plant BG, Recycling Center GC, Waste Transfer Facility SH, Waste Processing Complex SH, Waste Disposal Unit SH (CCP),
        //                                          Eco-Friendly Incinerator Plant HK (CCP)
        //                                          Has Outside Connections logic, but the logic always returns neutral color, so don't include this AI.
        //    UltimateRecyclingPlantAI              Ultimate Recycling Plant GC (monument)
        // LibraryAI                        GC      Public Library BG, Historical Library SR (CCP), National Library AM (CCP)
        // MainCampusBuildingAI             GC      Trade School Administration Building CA, Liberal Arts Administration Building CA, University Administration Building CA
        // MainIndustryBuildingAI           GC      Forestry Main Building IN, Farm Main Building IN, Ore Industry Main Building IN, Oil Industry Main Building IN
        // MaintenanceDepotAI               GC      Road Maintenance Depot SF, Park Maintenance Building PL
        // MarketAI                         GC  OC  Fish Market SH
        // MonumentAI                       GC      Landmarks:          ChirpX Launch Site BG
        // MonumentAI                       GC      Fin Districts:      Bronze Cow FD, Bronze Panda FD, Elevated Plaza FD, Underground Garden Plaza FD
        //                                          Landmarks:          Hypermarket BG, Government Offices BG, The Gherkin BG, London Eye BG, Sports Arena BG, Theatre BG, Shopping Center BG,
        //                                                              Cathedral BG, Amsterdam Palace BG, Winter Market BG, Department Store BG, City Hall BG, Cinema BG,
        //                                                              Panda Sanctuary PE, Oriental Pearl Tower PE, Temple Complex PE,
        //                                                              Traffic Park MT, Boat Museum MT, Locomotive Halls MT
        //                                          Deluxe Edition:     Statue of Liberty DE, Eiffel Tower DE, Grand Central Terminal DE, Arc de Triomphe DE, Brandenburg Gate DE
        //                                          Tourism & Leisure:  Icefishing Pond AD+SF, Casino AD, Driving Range AD, Fantastic Fountain AD, Frozen Fountain AD+SF, Luxury Hotel AD, Zoo AD
        //                                          Winter Unique:      Ice Hockey Arena SF, Ski Resort SF, Snowcastle Restaurant SF, Spa Hotel SF, Sleigh Ride SF, Snowboard Arena SF, The Christmas Tree SF, Igloo Hotel SF
        //                                          Pedestrian Area:    Pedestrian Street Market Hall PP, Museum of Post-Modern Art PP, Sunken Plaza Shopping Mall PP,
        //                                                              Commercial Zone Landmark PP, Residential Zone Landmark PP, Office Zone Landmark PP
        //                                          Match Day:          Football Stadium MD
        //                                          Concerts:           Media Broadcast Building CO, Music Club CO, Fan Zone Park CO
        //                                          Airports:           Aviation Museum AP
        //                                          Level 1 Unique:     Statue of Industry BG, Statue of Wealth BG, Lazaret Plaza BG, Statue of Shopping BG, Plaza of the Dead BG,
        //                                                              Meteorite Park ND, Bird and Bee Haven GC, City Arch PL
        //                                          Level 2 Unique:     Fountain of Life and Death BG, Friendly Neighborhood Park BG, Transport Tower BG, Mall of Moderation BG, Posh Mall BG,
        //                                                              Disaster Memorial ND, Climate Research Station GC, Clock Tower PL
        //                                          Level 3 Unique:     Colossal Order Offices BG, Official Park BG, Court House BG, Grand Mall BG, Tax Office BG,
        //                                                              Helicopter Park ND, Lungs of the City GC, Old Market Street PL
        //                                          Level 4 Unique:     Business Park BG, Grand Library BG, Observatory BG, Opera House BG, Oppression Office BG,
        //                                                              Pyramid Of Safety ND, Floating Gardens GC, Sea Fortress PL
        //                                          Level 5 Unique:     Servicing Services Offices BG, Academic Library BG, Science Center BG, Expo Center BG, High Interest Tower BG, Aquarium BG,
        //                                                              Sphinx Of Scenarios ND, Ziggurat Garden GC, Observation Tower PL
        //                                          Level 6 Unique:     Cathedral of Plenitude BG, Stadium BG, MAM Modern Art Museum BG, Sea-and-Sky Scraper BG, Theater of Wonders BG,
        //                                                              Sparkly Unicorn Rainbow Park ND, Central Park GC, The Statue of Colossalus PL
        //                                          CCP AR:             Eddie Kovanago, Pinoa Street, The Majesty
        //                                          CCP HT:             Electric Car Factory, Nanotechnology Center, Research Center, Robotics Institute, Semiconductor Plant,
        //                                                              Software Development Studio, Space Shuttle Launch Site, Television Station
        //                                          CCP MJ:             Drive-in Restaurant, Drive-in Oriental Restaurant, Oriental Market, Noodle Restaurant, Ramen Restaurant,
        //                                                              Service Station and Restaurant, Small Office Building, City Office Building, District Office Building,
        //                                                              Local Register Office, Resort Hotel, Downtown Hotel, The Station Department Store, The Rail Yard Shopping Center,
        //                                                              Temple, High-rise Office Building, Company Headquarters, Office Skyscraper
        //                                          CCP SR:             Coast Guard Heritage Museum, Hotel Lafayette, Hotel New Linwood, The Empire House, The Abbott Hotel,
        //                                                              Hotel Aldine, Hotel Lawrence, Hotel Colonial, Anchor House Inn, Hotel Vesper, The Atlantic Hotel,
        //                                                              Narragansett House, Hotel Brunswick, Ausable Chasm Hotel, The Fabyan House, Spring House, The Breakers Hotel,
        //                                                              Hotel Allaire, Ocean View Hotel, Isleworth Gardens, Hotel Fiske, Gordon Park Pavilion, Old Orchard House,
        //                                                              New Orchard Ocean Pier, Asbury Park Pavilion
        //                                          CCP SS:             Australia Triangle Building, Australia Poly Building, Large Bank Building, Marshalltown Centre,
        //                                                              Marshalltown Tower, Catalinas Norte Building, Catalinas Norte Tower, Torre Conde Paulista,
        //                                                              Torre Conde Joaquim, Dreischeibenturm, Von Pell Haus, Europaturm, Jubilee Motors Building,
        //                                                              Island Park Tower 56fl, Island Park Tower 44fl, Island Park Tower 68fl, Island Park Tower 32fl,
        //                                                              Marble Centre, Marble Building, MLM Centre, MLM Building, One Embarcadero, One Montgomery,
        //                                                              One Cheerful Plaza, One Galveston, Six Rivers Centre, National Trade Centre, Banqiao Tower,
        //                                                              Chihlee Tower, Torre Centrale, Torre Gioia, Gallusturm, Turm am Park, Torre Manilva,
        //                                                              Hinode Headquarters, Takeshiba Tower, Tour Bellini, Tour Courbevoie, Tour Saint-Denis,
        //                                                              Tour Finot, Tower 69, Tower 41, Moosach Turm, Olympia Turm
        //                                          CCP HK:             Acrocastle Apartment Complex, Chirp's Thumbs Up Plaza, Dosan Square Center, JANGBEESOFT R&D Center,
        //                                                              Korean Food Alley, Korean Style Temple, Mirae Department Store, Youjoy Entertainment Agency
        //                                          CCP SM:             Medium Grocery Store, Large Grocery Store, Open-Air Mall, Open-Air Mall Phase II, Shopping Plaza, Mall of Marvels
        //                                          CCP SV:             Medium Soccer Stadium, City Soccer Stadium, City Baseball Stadium, Medium Baseball Stadium,
        //                                                              Medium American Football Stadium, City American Football Stadium, Timber Box Soccer Stadium,
        //                                                              Copper Bowl, Horseshoe Stadium, Arrow Park, Peanut Bowl Memorial Stadium
        //                                          CCP AM:             Ego City Market, Conference Center, Ọrunmila Towers, Royal Museum, Sanctum of Oduduwa, The Temple of the Sahel,
        //                                                              Communications Center, Bantu Art Museum, Sahel Monument, The Gold Tower, Unity Pyramid
        //    AirlineHeadquartersAI                 Airline Headquarters Building AP
        //    AnimalMonumentAI                      Winter Unique:   Santa Claus' Workshop SF
        //    ChirpwickCastleAI                     Castle Of Lord Chirpwick PL (monument)
        //    FestivalAreaAI                        Festival Area CO
        //    InternationalTradeBuildingAI  GC      International Trade Building FD
        //    MuseumAI                              The Technology Museum CA, The Art Gallery CA, The Science Center CA
        //    PrivateAirportAI                      Aviation Club SH (Level 5 Unique)
        //    StockExchangeAI               GC      Stock Exchange FD
        //    VarsitySportsArenaAI          GC      Aquatics Center CA, Basketball Arena CA, Track And Field Stadium CA, Baseball Park CA, American Football Stadium CA
        // ParkAI                           GC      Parks:  Small Park BG, Small Playground BG, Park With Trees BG, Large Playground BG, Bouncy Castle Park BG, Botanical Garden BG,
        //                                                  Dog Park BG, Carousel Park BG, Japanese Garden BG, Tropical Garden BG, Fishing Island BG, Floating Cafe BG,
        //                                                  Snowmobile Track AD+SF, Winter Fishing Pier AD+SF, Ice Hockey Rink AD+SF
        //                                          Plazas:             Plaza with Trees BG, Plaza with Picnic Tables BG, Paradox Plaza BG (special)
        //                                          Other Parks:        Basketball Court BG, Tennis Court BG
        //                                          Tourism & Leisure:  Fishing Pier AD, Fishing Tours AD, Jet Ski Rental AD, Marina AD, Restaurant Pier AD, Beach Volleyball Court AD, Riding Stable AD, Skatepark AD
        //                                          Winter Parks:       Snowman Park SF, Ice Sculpture Park SF, Sledding Hill SF, Curling Park SF, Skating Rink SF, Ski Lodge SF, Cross-Country Skiing Park SF, Firepit Park SF
        //                                          CCP HT:             Biodome, Vertical Farm
        //                                          CCP BP:             Seine Pier, Rhine Pier
        //                                          CCP MM:             Car Port 2 Slot, Car Port 4 Slot, Car Port 6 Slot, Car Port 12 Slot, Car Port 24 Slot,
        //                                                              Hotel Oasis A, Hotel Oasis B, Motel Palm Springs, Roadside Diner, Mothership
        //                                          CCP SV:             Small Soccer Field, Community Soccer Park, Community Australian Football Field, Community Australian Football Park,
        //                                                              Community Baseball Field, Community Baseball Complex, Suburban American Football Field, Community American Football Park,
        //                                                              Suburban Cricket Pitch, Community Cricket Pitch
        //                                          CCP AM:             The Botanical Museum
        //    EdenProjectAI                         Eden Project BG (monument)
        // ParkBuildingAI                   GC      City Park:       PL: Park Plaza, Park Cafe #1, Park Restrooms #1, Park Info Booth #1, Park Chess Board #1, Park Pier #1, Park Pier #2
        //                                          Amusement Park:  PL: Amusement Park Plaza, Amusement Park Cafe #1, Amusement Park Souvenir Shop #1, Amusement Park Restrooms #1, Game Booth #1, Game Booth #2,
        //                                                               Carousel, Piggy Train, Rotating Tea Cups, Swinging Boat, House Of Horrors, Bumper Cars, Drop Tower Ride, Pendulum Ride, Ferris Wheel, Rollercoaster
        //                                          Zoo:             PL: Zoo Plaza, Zoo Cafe #1, Zoo Souvenir Shop #1, Zoo Restrooms #1, Moose And Reindeer Enclosure, Bird House, Antelope Enclosure, Bison Enclosure,
        //                                                               {Insect, Amphibian and Reptile House}, Flamingo Enclosure, Elephant Enclosure, Sealife Enclosure, Giraffe Enclosure, Monkey Palace, Rhino Enclosure, Lion Enclosure
        //                                          Nature Reserve:  PL: Campfire Site #1, Campfire Site #2, Tent #1, Tent #2, Tent #3, Viewing Deck #1, Viewing Deck #2, Tent Camping Site #1, Lean-To Shelter #1, Lean-To Shelter #2,
        //                                                               Lookout Tower #1, Lookout Tower #2, Camping Site #1, Fishing Cabin #1, Fishing Cabin #2, Hunting Cabin #1, Hunting Cabin #2, Bouldering Site #1
        //                                          Pedestrian:      PP: Small Food Truck Plaza, Small Fountain Plaza, Small Glass Roof Plaza, Statue Plaza, Large Food Truck Plaza,
        //                                                               Flower Plaza, Large Fountain Plaza, Large Glass Roof Plaza
        //    IceCreamStandAI                       Pedestrian:      PP: Small Ice Cream Stand Plaza, Large Ice Cream Stand Plaza
        // ParkGateAI                       GC      City Park:       PL: Park Main Gate, Small Park Main Gate, Park Side Gate
        //                                          Amusement Park:  PL: Amusement Park Main Gate, Small Amusement Park Main Gate, Amusement Park Side Gate
        //                                          Zoo:             PL: Zoo Main Gate, Small Zoo Main Gate, Zoo Side Gate
        //                                          Nature Reserve:  PL: Nature Reserve Main Gate, Small Nature Reserve Main Gate, Nature Reserve Side Gate
        // PoliceStationAI                  GC      Police Station BG, Police Headquarters BG, High-Capacity Police Headquarters PP, Prison AD, Historical Police Station SR (CCP), Intelligence Agency HT (CCP),
        //                                          Police Security Center HK (CCP), Police Department AM (CCP)
        // PostOfficeAI                     GC  OC  Post Office IN, Post Sorting Facility IN
        // PowerPlantAI                     GC  OC  Coal Power Plant BG, Oil Power Plant BG, Nuclear Power Plant BG, Geothermal Power Plant GC, Ocean Thermal Energy Conversion Plant GC
        //                                          (unlimited coal/oil reserves so cannot compute storage)
        //    DamPowerHouseAI                       Hydro Power Plant BG
        //    FusionPowerPlantAI                    Fusion Power Plant BG (monument)
        //    SolarPowerPlantAI                     Solar Power Plant BG, Solar Updraft Tower GC
        //    WindTurbineAI                 GC      Wind Turbine BG, Advanced Wind Turbine BG, Wave Power Plant HT (CCP)
        // RadioMastAI                      GC      Short Radio Mast ND, Tall Radio Mast ND
        // SaunaAI                          GC      Sauna SF, Sports Hall and Gymnasium GC, Community Pool GC, Yoga Garden GC
        // SchoolAI                         GC      Elementary School BG, High-Capacity Elementary School PP, High School BG, High-Capacity High School PP, University BG, High-Capacity University PP,
        //                                          Community School GC, Institute of Creative Arts GC, Modern Technology Institute GC, Faculty HT (CCP), Large Elementary School HK (CCP), Community School AM (CCP)
        //    CampusBuildingAI              GC      Trade School:   CA: Trade School Dormitory, Trade School Study Hall, Trade School Groundskeeping, Book Club, Trade School Outdoor Study, Trade School Gymnasium, Trade School Cafeteria,
        //                                                              Trade School Fountain, Trade School Library, IT Club, Trade School Commencement Office, Trade School Academic Statue 1, Trade School Auditorium, Trade School Laboratories,
        //                                                              Trade School Bookstore, Trade School Media Lab, Beach Volleyball Club, Trade School Academic Statue 2
        //                                          Liberal Arts:   CA: Liberal Arts Dormitory, Liberal Arts Study Hall, Liberal Arts Groundskeeping, Drama Club, Liberal Arts Outdoor Study, Liberal Arts Gymnasium, Liberal Arts Cafeteria,
        //                                                              Liberal Arts Fountain, Liberal Arts Library, Art Club, Liberal Arts Commencement Office, Liberal Arts Academic Statue 1, Liberal Arts Auditorium, Liberal Arts Laboratories,
        //                                                              Liberal Arts Bookstore, Liberal Arts Media Lab, Dance Club, Liberal Arts Academic Statue 2
        //                                          University:     CA: University Dormitory, University Study Hall, University Groundskeeping, Futsal Club, University Outdoor Study, University Gymnasium, University Cafeteria
        //                                                              University Fountain, University Library, Math Club, University Commencement Office, University Academic Statue 1, University Auditorium, University Laboratories,
        //                                                              University Bookstore, University Media Lab, Chess Club, University Academic Statue 2
        //       UniqueFacultyAI                    Trade School:   CA: Police Academy, School of Tourism And Travel, School of Engineering
        //                                          Liberal Arts:   CA: School of Education, School of Environmental Studies, School of Economics
        //                                          University:     CA: School of Law, School of Medicine, School of Science
        // ServicePointAI                   GC      Small Pedestrian Area Service Point PP, Large Pedestrian Area Service Point PP, Small Cargo Service Point PP, Large Cargo Service Point PP,
        //                                          Small Garbage Service Point PP, Large Garbage Service Point PP
        // ShelterAI                        GC  OC  Small Emergency Shelter ND, Large Emergency Shelter ND
        // SnowDumpAI                       GC      Snow Dump SF
        // SpaceElevatorAI                  GC      Space Elevator BG (monument)
        // SpaceRadarAI                     GC      Deep Space Radar ND
        // TaxiStandAI                      GC      Taxi Stand AD (taxis wait at a Taxi Stand for a customer, taxis are not generated by a Taxi Stand)
        // TollBoothAI                      GC      Two-Way Toll Booth BG, One-Way Toll Booth BG, Two-Way Large Toll Booth BG, One-Way Large Toll Booth BG
        // TourBuildingAI                   GC      Hot Air Balloon Tours PL
        //    ChirperTourAI                         Chirper Balloon Tours BG
        // TsunamiBuoyAI                    GC      Tsunami Warning Buoy ND
        // WarehouseAI                      GC  OC  Forestry:  IN: Small Log Yard, Saw Dust Storage, Large Log Yard, Wood Chip Storage
        //                                          Farming:   IN: Small Grain Silo, Large Grain Silo, Small Barn, Large Barn
        //                                          Ore:       IN: Sand Storage, Ore Storage, Ore Industry Storage, Raw Mineral Storage
        //                                          Oil:       IN: Small Crude Oil Tank Farm, Large Crude Oil Tank Farm, Crude Oil Storage Cavern, Oil Industry Storage
        //                                          Generic:   IN: Warehouse Yard, Small Warehouse, Medium Warehouse, Large Warehouse
        // WaterCleanerAI                   GC      Floating Garbage Collector GC
        // WaterFacilityAI                  GC      Water Pumping Station BG, Water Tower BG, Large Water Tower SH, Water Drain Pipe BG, Water Treatment Plant BG,
        //                                          Inland Water Treatment Plant SH, Advanced Inland Water Treatment Plant SH, Eco Water Outlet GC, Eco Water Treatment Plant GC,
        //                                          Eco Inland Water Treatment Plant SH, Eco Advanced Inland Water Treatment Plant SH, Fresh Water Outlet ND
        // WaterFacilityAI                  GC      Tank Reservoir ND
        // WaterFacilityAI                  GC      Pumping Service ND
        // WeatherRadarAI                   GC      Weather Radar ND




        /// <summary>
        /// create a patch for every building AI that has a GetColor method with logic for Outside Connections
        /// in the listings above, that is building AIs marked with GC and OC
        /// </summary>
        public static bool CreateGetColorPatches()
        {
            if (!CreateGetColorPatch<CommercialBuildingAI >()) return false;
            if (!CreateGetColorPatch<OfficeBuildingAI     >()) return false;
            if (!CreateGetColorPatch<IndustrialBuildingAI >()) return false;
            if (!CreateGetColorPatch<IndustrialExtractorAI>()) return false;

            if (!CreateGetColorPatch<FishFarmAI           >()) return false;
            if (!CreateGetColorPatch<FishingHarborAI      >()) return false;
            if (!CreateGetColorPatch<HeatingPlantAI       >()) return false;
            if (!CreateGetColorPatch<ExtractingFacilityAI >()) return false;
            if (!CreateGetColorPatch<ProcessingFacilityAI >()) return false;
            if (!CreateGetColorPatch<MarketAI             >()) return false;
            if (!CreateGetColorPatch<PostOfficeAI         >()) return false;
            if (!CreateGetColorPatch<PowerPlantAI         >()) return false;
            if (!CreateGetColorPatch<ShelterAI            >()) return false;
            if (!CreateGetColorPatch<WarehouseAI          >()) return false;

            // success
            return true;
        }

        /// <summary>
        /// create a patch of the GetColor method for the specified building AI type
        /// </summary>
        private static bool CreateGetColorPatch<T>() where T : CommonBuildingAI
        {
            // same routine is used for all building AI types
            return HarmonyPatcher.CreatePrefixPatch(typeof(T), "GetColor", BindingFlags.Instance | BindingFlags.Public, typeof(BuildingAIPatch), "BuildingAIGetColor");
        }

        /// <summary>
        /// return the color of the building
        /// same routine is used for all building AI types
        /// </summary>
        /// <returns>whether or not to do base processing</returns>
        public static bool BuildingAIGetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, ref Color __result)
        {
            // do processing for this mod only for Outside Connections info view
            if (infoMode == InfoManager.InfoMode.Connections)
            {
                return EOCVUserInterface.instance.GetBuildingColor(buildingID, ref data, ref __result);
            }

            // do the base processing
            return true;
        }
    }
}
