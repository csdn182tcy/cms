﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Core.Utils;
using SSCMS.Models;
using SSCMS.Utils;

namespace SSCMS.Web.Controllers.Admin.Cms.Editor
{
    public partial class EditorController
    {
        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> Get([FromQuery]GetRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId,
                    AuthTypes.SitePermissions.Contents) ||
                !await _authManager.HasContentPermissionsAsync(request.SiteId, request.ChannelId, AuthTypes.SiteContentPermissions.Add) ||
                !await _authManager.HasContentPermissionsAsync(request.SiteId, request.ChannelId, AuthTypes.SiteContentPermissions.Edit))
            {
                return Unauthorized();
            }

            var site = await _siteRepository.GetAsync(request.SiteId);
            if (site == null) return NotFound();

            var channel = await _channelRepository.GetAsync(request.ChannelId);
            if (channel == null) return NotFound();

            var groupNames = await _contentGroupRepository.GetGroupNamesAsync(site.Id);
            var tagNames = await _contentTagRepository.GetTagNamesAsync(site.Id);

            var tableName = _channelRepository.GetTableName(site, channel);
            var allStyles = await _tableStyleRepository.GetContentStyleListAsync(channel, tableName);
            var styles = allStyles
                .Where(style =>
                    !string.IsNullOrEmpty(style.DisplayName) &&
                    !StringUtils.ContainsIgnoreCase(ColumnsManager.MetadataAttributes.Value, style.AttributeName))
                .Select(x => new InputStyle(x));

            var (userIsChecked, userCheckedLevel) = await CheckManager.GetUserCheckLevelAsync(_authManager, site, site.Id);
            var checkedLevels = CheckManager.GetCheckedLevelOptions(site, userIsChecked, userCheckedLevel, true);

            Content content;
            if (request.ContentId > 0)
            {
                content = await _pathManager.DecodeContentAsync(site, channel, request.ContentId);
            }
            else
            {
                content = new Content
                {
                    Id = 0,
                    SiteId = site.Id,
                    ChannelId = channel.Id,
                    AddDate = DateTime.Now,
                    CheckedLevel = site.CheckContentDefaultLevel
                };
            }

            //await ContentUtility.TextEditorContentDecodeAsync(parseManager.PathManager, pageInfo.Site, content.Get<string>(ContentAttribute.Content), pageInfo.IsLocal);

            return new GetResult
            {
                Content = content,
                Site = site,
                Channel = channel,
                GroupNames = groupNames,
                TagNames = tagNames,
                Styles = styles,
                CheckedLevels = checkedLevels
            };
        }
    }
}
