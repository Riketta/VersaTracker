using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaTracker
{
    class ConnectedRealms
    {
        static Dictionary<string, int> realmIds = new Dictionary<string, int>();

        public static void Update(WarcraftAPI api)
        {
            int pages = 1;
            for (int i = 1; i <= pages; i++)
            {
                var response = api.ConnectedRealmsListApiRequest(i);
                pages = response.pageCount;

                foreach (var result in response.results)
                    foreach (var realm in result.data.realms)
                        realmIds[realm.slug] = result.data.id;
            }
        }

        public static int GetRealmIdBySlug(string slug)
        {
            return realmIds[slug];
        }

        public static string[] GetRealmSlugsById(int id)
        {
            return realmIds.Where(i => i.Value == id).Select(i => i.Key).ToArray();
        }
    }
}
