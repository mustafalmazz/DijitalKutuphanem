using BookManagementApp.Areas.Admin.Models;
using System.Collections.Generic;

namespace BookManagementApp.Models
{
    public class StoreViewModel
    {
        public IEnumerable<ProfileFrame> Frames { get; set; }
        public IEnumerable<ProfileAvatar> Avatars { get; set; }
        public IEnumerable<ProfileAvatar> PackAvatars { get; set; }

        /// <summary>
        /// Tüm paketlerin içeriği, tür bağımsız biçimde (avatar + çerçeve + banner).
        /// Paket görünümü bunu CategoryCode'a göre süzer.
        /// </summary>
        public IEnumerable<PackItemView> PackItems { get; set; } = new List<PackItemView>();
        public IEnumerable<StorePackage> Packages { get; set; }
        public IEnumerable<ProfileBanner> Banners { get; set; } = new List<ProfileBanner>();
    }
}
