﻿//    This file is part of Nfield.SDK.
//
//    Nfield.SDK is free software: you can redistribute it and/or modify
//    it under the terms of the GNU Lesser General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Nfield.SDK is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public License
//    along with Nfield.SDK.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Nfield.Extensions;
using Nfield.Infrastructure;
using Nfield.Utilities;

namespace Nfield.Services.Implementation
{
    internal class NfieldSurveyScriptFragmentService : INfieldSurveyScriptFragmentService, INfieldConnectionClientObject
    {
        #region Implementation of INfieldSurveyScriptFragmentService

        public Task<IQueryable<string>> QueryAsync(string surveyId)
        {
            Ensure.ArgumentNotNullOrEmptyString(surveyId, nameof(surveyId));

            return Client.GetAsync(FragmentsApi(surveyId, null))
                .ContinueWith(responseMessageTask => responseMessageTask.Result.Content.ReadAsStringAsync().Result)
                .ContinueWith(stringTask => JsonConvert.DeserializeObject<List<string>>(stringTask.Result).AsQueryable())
                .FlattenExceptions();
        }

        public Task<string> GetAsync(string surveyId, string fileName)
        {
            Ensure.ArgumentNotNullOrEmptyString(surveyId, nameof(surveyId));
            Ensure.ArgumentNotNullOrEmptyString(fileName, nameof(fileName));

            return Client.GetAsync(FragmentsApi(surveyId, fileName))
                .ContinueWith(responseMessageTask => responseMessageTask.Result.Content.ReadAsStringAsync())
                .ContinueWith(b => b.Result.Result)
                .FlattenExceptions();
        }

        public Task AddOrUpdateAsync(string surveyId, string fileName, string script)
        {
            Ensure.ArgumentNotNullOrEmptyString(surveyId, nameof(surveyId));
            Ensure.ArgumentNotNullOrEmptyString(fileName, nameof(fileName));
            Ensure.ArgumentNotNull(script, nameof(script));

            var postContent = new StringContent(script, Encoding.Unicode);
            return Client.PostAsync(FragmentsApi(surveyId, fileName), postContent)
                .FlattenExceptions();
        }

        public Task RemoveAsync(string surveyId, string fileName)
        {
            Ensure.ArgumentNotNullOrEmptyString(surveyId, nameof(surveyId));
            Ensure.ArgumentNotNullOrEmptyString(fileName, nameof(fileName));

            return Client.DeleteAsync(FragmentsApi(surveyId, fileName))
                .FlattenExceptions();
        }

        #endregion

        #region Implementation of INfieldConnectionClientObject

        public INfieldConnectionClient ConnectionClient { get; internal set; }

        public void InitializeNfieldConnection(INfieldConnectionClient connection)
        {
            ConnectionClient = connection;
        }

        #endregion

        private INfieldHttpClient Client
        {
            get { return ConnectionClient.Client; }
        }

        private Uri FragmentsApi(string surveyId, string fileName)
        {
            var path = new StringBuilder();
            path.AppendFormat("Surveys/{0}/ScriptFragments/", surveyId);
            if (!string.IsNullOrEmpty(fileName))
            {
                path.Append(HttpUtility.UrlEncode(fileName));
            }
            return new Uri(ConnectionClient.NfieldServerUri, path.ToString());
        }

    }
}
