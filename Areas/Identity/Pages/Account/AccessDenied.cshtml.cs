// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using NuJournalPro.Models;
using NuJournalPro.Services.Interfaces;

namespace NuJournalPro.Areas.Identity.Pages.Account
{
    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class AccessDeniedModel : PageModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        private readonly IImageService _imageService;
        private readonly DefaultGraphics _defaultGraphics;

        public AccessDeniedModel(IImageService imageService,
                                 IOptions<DefaultGraphics> defaultGraphics)
        {
            _imageService = imageService;
            _defaultGraphics = defaultGraphics.Value;
        }

        public byte[] ImageData { get; set; }
        public string MimeType { get; set; }

        public async Task<IActionResult> OnGet()
        {
            ImageData = await _imageService.EncodeImageAsync(_defaultGraphics.SecureAccess);
            MimeType = _imageService.MimeType(_defaultGraphics.SecureAccess);
            return Page();
        }
    }
}
