using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RobsonAraujo.Demos.ASPNetCoreFaceAPI.Pages
{
    public class FaceLoginModel : PageModel
    {
        private const string personGroupId = "usersgroupid";

        private readonly IUserService _userService;
        private readonly IFaceService _faceService;
        public User IdentifiedUser { get; set; }
        public string UploadedPicture {get; set;}
        public FaceLoginModel(IUserService userService, IFaceService faceService)
        {
            _faceService = faceService;
            _userService = userService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await _faceService.InitializePersonGroupAsync(personGroupId, "Users Person Group", _userService.Users);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string dataUrl)
        {
            var imageBytes = Convert.FromBase64String(dataUrl.Split(',')[1]);
            IdentifiedUser = await _faceService.IdentifyPersonAsync(personGroupId, 0.5, imageBytes, _userService.Users);
            UploadedPicture = dataUrl;
            return Page();
        }
    }
}