using TrainLines;

namespace Packages
{
    public class PackageWaitingList
    {
        public Station loadingStation;
        public Dictionary<Station, List<Package>> waitingPackages;
        public PackageWaitingList(Station loadingStation)
        {
            this.loadingStation = loadingStation;
            waitingPackages = new Dictionary<Station, List<Package>>();
        }

        /// <summary>
        /// Adds a package to the waiting list. Returns false when it cant be added and true when added
        /// </summary>
        public bool addPackageToList(Package package)
        {
            if (package.sourceStation == loadingStation)
            {
                //Check if source station is there
                if (waitingPackages.ContainsKey(package.targetStation))
                {
                    waitingPackages[package.targetStation].Add(package);
                }
                else
                {
                    waitingPackages.Add(package.targetStation, new List<Package>() { package });
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds a range of packages to the package list
        /// </summary>
        public void addPackeRangeToList(List<Package> newPackages)
        {
            foreach (Package package in newPackages)
            {
                if (addPackageToList(package) == false)
                {
                    throw new Exception("Package cant be added to list");
                }
            }
        }


        /// <summary>
        /// Gets the total number of packages waiting in the list
        /// </summary>
        public int getNumberOfPackagesWaiting()
        {
            int count = 0;
            foreach (KeyValuePair<Station, List<Package>> waitingLists in waitingPackages)
            {
                count += waitingLists.Value.Count;
            }

            return count;
        }


        /// <summary>
        /// Returns the station where most packages are waiting
        /// </summary>
        public Station targetStationWithMostPackagesWaiting()
        {
            if (hasWaitingPackages())
            {
                Station targetStation = waitingPackages.OrderByDescending(kvp => kvp.Value.Count).First().Key;
                return targetStation;
            }
            throw new Exception("No Packages waiting");
        }


        /// <summary>
        /// Checks if the package waiting list has packages to deliver
        /// </summary>
        public bool hasWaitingPackages()
        {
            if (getNumberOfPackagesWaiting() <= 0)
            {
                return false;
            }
            return true;
        }



        /// <summary>
        /// Removes a single package from the waiting list
        /// </summary>
        public bool removePackage(Package packageToRemove)
        {
            if (waitingPackages.ContainsKey(packageToRemove.targetStation) && packageToRemove.sourceStation == loadingStation)
            {
                if (waitingPackages[packageToRemove.targetStation].Contains(packageToRemove))
                {
                    waitingPackages[packageToRemove.targetStation].Remove(packageToRemove);

                    //Check if there are still packages left, if not remove the entry in the dictionary
                    if (waitingPackages[packageToRemove.targetStation].Count == 0)
                    {
                        waitingPackages.Remove(packageToRemove.targetStation);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }


        /// <summary>
        /// Removes a range of packages from the package list
        /// </summary>
        public void removePackageRange(List<Package> packagesToRemove)
        {
            foreach (Package package in packagesToRemove)
            {
                if (removePackage(package) == false)
                {
                    throw new Exception("Cant remove package from waiting list");
                }
            }
        }


        public string getPackageDestinationListJSON()
        {
            string str = "";
            foreach (KeyValuePair<Station, List<Package>> destinationList in waitingPackages)
            {
                foreach (Package package in destinationList.Value)
                {
                    str += "\"" + package.targetStation.triasID + "\"";
                    if (waitingPackages.Last().Value.Last() != package)
                    {
                        str += ",";
                    }

                    str += "\n";
                }
            }

            return str;
        }
    }
}