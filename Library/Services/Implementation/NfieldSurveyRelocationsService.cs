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
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nfield.Extensions;
using Nfield.Infrastructure;
using Nfield.Models;

namespace Nfield.Services.Implementation
{
    /// <summary>
    /// Implementation of <see cref="INfieldSurveyRelocationsService"/>
    /// </summary>
    internal class NfieldSurveyRelocationsService : INfieldSurveyRelocationsService, INfieldConnectionClientObject
    {
        #region Implementation of INfieldRelocationsService

        /// <summary>
        /// See <see cref="INfieldSurveyRelocationsService.QueryAsync"/>
        /// </summary>
        public Task<IQueryable<SurveyRelocation>> QueryAsync(string surveyId)
        {
            CheckSurveyId(surveyId);

            return Client.GetAsync(RelocationsApi(surveyId))
                         .ContinueWith(
                             responseMessageTask => responseMessageTask.Result.Content.ReadAsStringAsync().Result)
                         .ContinueWith(
                             stringTask =>
                             JsonConvert.DeserializeObject<List<SurveyRelocation>>(stringTask.Result).AsQueryable())
                         .FlattenExceptions();
        }

        /// <summary>
        /// See <see cref="INfieldSurveyRelocationsService.UpdateAsync"/>
        /// </summary>
        public Task UpdateAsync(string surveyId, SurveyRelocation relocation)
        {
            CheckSurveyId(surveyId);

            if (relocation == null)
            {
                throw new ArgumentNullException(nameof(relocation));
            }

            return Client.PutAsJsonAsync(RelocationsApi(surveyId), relocation)
                         .ContinueWith(task => task.Result.Content.ReadAsStringAsync().Result)
                         .ContinueWith(task => JsonConvert.DeserializeObject<SurveyRelocation>(task.Result))
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

        private static void CheckSurveyId(string surveyId)
        {
            if (surveyId == null)
                throw new ArgumentNullException(nameof(surveyId));
            if (surveyId.Trim().Length == 0)
                throw new ArgumentException("surveyId cannot be empty");
        }

        private INfieldHttpClient Client => ConnectionClient.Client;

        private Uri RelocationsApi(string surveyId)
        {
            return new Uri(ConnectionClient.NfieldServerUri, $"Surveys/{surveyId}/Relocations");
        }
    }
}
