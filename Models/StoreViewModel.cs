using BookManagementApp.Areas.Admin.Models;
using System.Collections.Generic;

namespace BookManagementApp.Models
{
    public class StoreViewModel
    {
        public IEnumerable<ProfileFrame> Frames { get; set; }
        public IEnumerable<ProfileAvatar> Avatars { get; set; }
        public IEnumerable<ProfileAvatar> PackAvatars { get; set; }
        public IEnumerable<StorePackage> Packages { get; set; }
    }
}
