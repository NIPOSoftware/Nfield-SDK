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
using System.Linq;
using System.Net;
using System.Net.Http;
using Moq;
using Newtonsoft.Json;
using Nfield.Infrastructure;
using Nfield.Models;
using Nfield.Services.Implementation;
using Xunit;

namespace Nfield.Services
{
    /// <summary>
    /// Tests for <see cref="NfieldSurveysService"/>
    /// </summary>
    public class NfieldSurveysServiceTests : NfieldServiceTestsBase
    {
        #region QueryAsync

        [Fact]
        public void TestQueryAsync_ServerReturnsQuery_ReturnsListWithSurveys()
        {
            var expectedSurveys = new Survey[]
            { new Survey { SurveyId = "TestSurvey" },
              new Survey { SurveyId = "AnotherTestSurvey" }
            };
            var mockedNfieldConnection = new Mock<INfieldConnectionClient>();
            var mockedHttpClient = CreateHttpClientMock(mockedNfieldConnection);
            mockedHttpClient
                .Setup(client => client.GetAsync(ServiceAddress + "surveys/"))
                .Returns(CreateTask(HttpStatusCode.OK, new StringContent(JsonConvert.SerializeObject(expectedSurveys))));

            var target = new NfieldSurveysService();
            target.InitializeNfieldConnection(mockedNfieldConnection.Object);

            var actualSurveys = target.QueryAsync().Result;

            Assert.Equal(expectedSurveys[0].SurveyId, actualSurveys.ToArray()[0].SurveyId);
            Assert.Equal(expectedSurveys[1].SurveyId, actualSurveys.ToArray()[1].SurveyId);
            Assert.Equal(2, actualSurveys.Count());
        }

        #endregion

        #region AddAsync

        [Fact]
        public void TestAddAsync_SurveyIsNull_ThrowsArgumentNullException()
        {
            var target = new NfieldSurveysService();
            Assert.Throws<ArgumentNullException>(() => UnwrapAggregateException(target.RemoveAsync(null)));
        }

        [Fact]
        public void TestAddAsync_ServerAccepts_ReturnsSurvey()
        {
            var survey = new Survey { SurveyName = "New Survey", SurveyType = SurveyType.Basic };
            var mockedNfieldConnection = new Mock<INfieldConnectionClient>();
            var mockedHttpClient = CreateHttpClientMock(mockedNfieldConnection);
            var content = new StringContent(JsonConvert.SerializeObject(survey));
            mockedHttpClient
                .Setup(client => client.PostAsJsonAsync(ServiceAddress + "surveys/", survey))
                .Returns(CreateTask(HttpStatusCode.OK, content));

            var target = new NfieldSurveysService();
            target.InitializeNfieldConnection(mockedNfieldConnection.Object);

            var actual = target.AddAsync(survey).Result;

            Assert.Equal(survey.SurveyName, actual.SurveyName);
            Assert.Equal(survey.SurveyType, actual.SurveyType);
        }

        #endregion

        #region RemoveAsync

        [Fact]
        public void TestRemoveAsync_SurveyIsNull_ThrowsArgumentNullException()
        {
            var target = new NfieldSurveysService();
            Assert.Throws<ArgumentNullException>(() => UnwrapAggregateException(target.RemoveAsync(null)));
        }

        [Fact]
        public void TestRemoveAsync_ServerRemovedSurvey_DoesNotThrow()
        {
            const string SurveyId = "Survey X";
            var survey = new Survey { SurveyId = SurveyId };
            var mockedNfieldConnection = new Mock<INfieldConnectionClient>();
            var mockedHttpClient = CreateHttpClientMock(mockedNfieldConnection);
            mockedHttpClient
                .Setup(client => client.DeleteAsync(ServiceAddress + "surveys/" + SurveyId))
                .Returns(CreateTask(HttpStatusCode.OK));

            var target = new NfieldSurveysService();
            target.InitializeNfieldConnection(mockedNfieldConnection.Object);

            Assert.DoesNotThrow(() => target.RemoveAsync(survey).Wait());
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public void TestUpdateAsync_SurveyArgumentIsNull_ThrowsArgumentNullException()
        {
            var target = new NfieldSurveysService();
            Assert.Throws<ArgumentNullException>(() => UnwrapAggregateException(target.UpdateAsync(null)));
        }

        [Fact]
        public void TestUpdateAsync_InterviewerExists_ReturnsInterviewer()
        {
            const string surveyId = "aSurveyId";
            var survey = new Survey
            {
                SurveyId = surveyId,
                Description = "updated description"
            };
            var mockedNfieldConnection = new Mock<INfieldConnectionClient>();
            var mockedHttpClient = CreateHttpClientMock(mockedNfieldConnection);
            mockedHttpClient
                .Setup(client => client.PatchAsJsonAsync(ServiceAddress + "surveys/" + surveyId, It.IsAny<UpdateSurvey>()))
                .Returns(CreateTask(HttpStatusCode.OK, new StringContent(JsonConvert.SerializeObject(survey))));

            var target = new NfieldSurveysService();
            target.InitializeNfieldConnection(mockedNfieldConnection.Object);

            var actual = target.UpdateAsync(survey).Result;

            Assert.Equal(survey.Description, actual.Description);
        }

        #endregion

        #region QuotaQueryAsync

        [Fact]
        public void TestQuotaQueryAsync_ServerReturnsQuery_ReturnsListWithQuotaLevel()
        {
            const string levelId = "LevelId";
            const string name = "Name";

            var expectedQuotaLevel = new QuotaLevel
            { 
                Id = levelId,
                Name = name
            };
            var mockedNfieldConnection = new Mock<INfieldConnectionClient>();
            var mockedHttpClient = CreateHttpClientMock(mockedNfieldConnection);
            mockedHttpClient
                .Setup(client => client.GetAsync(ServiceAddress + "surveys/1/quota"))
                .Returns(CreateTask(HttpStatusCode.OK, new StringContent(JsonConvert.SerializeObject(expectedQuotaLevel))));

            var target = new NfieldSurveysService();
            target.InitializeNfieldConnection(mockedNfieldConnection.Object);

            var actualQuotaLevel = target.QuotaQueryAsync("1").Result;

            Assert.Equal(expectedQuotaLevel.Id, actualQuotaLevel.Id);
            Assert.Equal(expectedQuotaLevel.Name, actualQuotaLevel.Name);

        }

        #endregion

        #region SamplingPointQueryAsync

        [Fact]
        public void TestSamplingPointQueryAsync_ServerReturnsQuery_ReturnsListWithSamplingPoint()
        {
            var expectedSamplingPoint = new SamplingPoint[]
            { new SamplingPoint { SamplingPointId = "SamplingPointId" },
              new SamplingPoint { SamplingPointId = "AnotherSamplingPointId" }
            };
            var mockedNfieldConnection = new Mock<INfieldConnectionClient>();
            var mockedHttpClient = CreateHttpClientMock(mockedNfieldConnection);
            mockedHttpClient
                .Setup(client => client.GetAsync(ServiceAddress + "surveys/1/samplingpoints"))
                .Returns(CreateTask(HttpStatusCode.OK, new StringContent(JsonConvert.SerializeObject(expectedSamplingPoint))));

            var target = new NfieldSurveysService();
            target.InitializeNfieldConnection(mockedNfieldConnection.Object);

            var actualSamplingPoint = target.SamplingPointsQueryAsync("1").Result;

            Assert.Equal(expectedSamplingPoint[0].SamplingPointId, actualSamplingPoint.ToArray()[0].SamplingPointId);
            Assert.Equal(expectedSamplingPoint[1].SamplingPointId, actualSamplingPoint.ToArray()[1].SamplingPointId);
            Assert.Equal(2, actualSamplingPoint.Count());
        }

        #endregion

        #region SamplingPointAddAsync

        [Fact]
        public void TestSamplingPointAddAsync_ServerAcceptsSamplingPoint_ReturnsSamplingPoint()
        {
            var office = new FieldworkOffice { OfficeId = "OfficeId" };
            var survey = new Survey { SurveyId = "SurveyId" };
            var samplingPoint = new SamplingPoint
            {
                SamplingPointId = "SamplingPointId",
                FieldworkOfficeId = office.OfficeId
            };

            var mockedNfieldConnection = new Mock<INfieldConnectionClient>();
            var mockedHttpClient = CreateHttpClientMock(mockedNfieldConnection);
            var content = new StringContent(JsonConvert.SerializeObject(samplingPoint));
            mockedHttpClient
                .Setup(
                    client =>
                        client.PostAsJsonAsync(
                            string.Format("{0}surveys/{1}/samplingpoints", ServiceAddress, survey.SurveyId),
                            samplingPoint))
                .Returns(CreateTask(HttpStatusCode.OK, content));

            var target = new NfieldSurveysService();
            target.InitializeNfieldConnection(mockedNfieldConnection.Object);

            var actual = target.SamplingPointAddAsync(survey.SurveyId, samplingPoint).Result;

            Assert.Equal(samplingPoint.SamplingPointId, actual.SamplingPointId);
            Assert.Equal(samplingPoint.FieldworkOfficeId, actual.FieldworkOfficeId);
        }

        #endregion

        #region SamplingPointUpdateAsync
        
        [Fact]
        public void TestSamplingPointUpdateAsync_SamplingPointArgumentIsNull_ThrowsArgumentNullException()
        {
            var target = new NfieldSurveysService();
            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    try
                    {
                        target.SamplingPointUpdateAsync("", null).Wait();
                    }
                    catch (AggregateException ex)
                    {
                        throw ex.InnerException;
                    }
                }
                );
        }

        [Fact]
        public void TestSamplingPointUpdateAsync_SamplingPointExists_ReturnsSamplingPoint()
        {
            const string surveyId = "SurveyId";
            const string samplingPointId = "SamplingPointId";

            var samplingPoint = new SamplingPoint
            {
                SamplingPointId = samplingPointId,
                Name = "Updated"
            };
            var mockedNfieldConnection = new Mock<INfieldConnectionClient>();
            var mockedHttpClient = CreateHttpClientMock(mockedNfieldConnection);
            mockedHttpClient
                .Setup(
                    client =>
                        client.PatchAsJsonAsync<UpdateSamplingPoint>(
                            string.Format("{0}surveys/{1}/samplingpoints/{2}", ServiceAddress, surveyId,
                                samplingPointId), It.IsAny<UpdateSamplingPoint>()))
                .Returns(CreateTask(HttpStatusCode.OK,
                    new StringContent(JsonConvert.SerializeObject(samplingPoint))));

            var target = new NfieldSurveysService();
            target.InitializeNfieldConnection(mockedNfieldConnection.Object);

            var actual = target.SamplingPointUpdateAsync(surveyId, samplingPoint).Result;

            Assert.Equal(samplingPoint.Name, actual.Name);
        }

        #endregion

        #region SamplingPointRemoveAsync

        [Fact]
        public void TestSamplingPointRemoveAsync_SamplingPointIdIsNull_ThrowsArgumentNullException()
        {
            var target = new NfieldSurveysService();
            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    try
                    {
                        target.SamplingPointDeleteAsync("", null).Wait();
                    }
                    catch (AggregateException ex)
                    {
                        throw ex.InnerException;
                    }
                }
                );
        }

        [Fact]
        public void TestSamplingPointRemoveAsync_ServerRemovedSamplingPoint_DoesNotThrow()
        {
            const string samplingPointId = "SamplingPointId";
            const string surveyId = "SurveyId";
            var samplingPoint = new SamplingPoint
            {
                SamplingPointId = samplingPointId
            };
            var mockedNfieldConnection = new Mock<INfieldConnectionClient>();
            var mockedHttpClient = CreateHttpClientMock(mockedNfieldConnection);
            mockedHttpClient
                .Setup(client => client.DeleteAsync(string.Format("{0}surveys/{1}/samplingpoints/{2}", ServiceAddress, surveyId,
                                samplingPointId)))
                .Returns(CreateTask(HttpStatusCode.OK));

            var target = new NfieldSurveysService();
            target.InitializeNfieldConnection(mockedNfieldConnection.Object);

            Assert.DoesNotThrow(() => target.SamplingPointDeleteAsync(surveyId, samplingPoint).Wait());
        }

        #endregion

        #region SamplingPointQuotaTargetsQueryAsync

        [Fact]
        public void TestSamplingPointQuotaTargetsQueryAsync_ServerReturnsQuery_ReturnsListWithSamplingPointQuotaTargets()
        {
            var expectedSamplingPointQuotaTarget = new SamplingPointQuotaTarget[]
            { new SamplingPointQuotaTarget { LevelId = "TestLevel" },
              new SamplingPointQuotaTarget { LevelId = "AnotherTestLevel" }
            };
            var mockedNfieldConnection = new Mock<INfieldConnectionClient>();
            var mockedHttpClient = CreateHttpClientMock(mockedNfieldConnection);
            mockedHttpClient
                .Setup(client => client.GetAsync(ServiceAddress + "surveys/1/samplingpoints/1/quotatargets"))
                .Returns(CreateTask(HttpStatusCode.OK, new StringContent(JsonConvert.SerializeObject(expectedSamplingPointQuotaTarget))));

            var target = new NfieldSurveysService();
            target.InitializeNfieldConnection(mockedNfieldConnection.Object);

            var actualSamplingPointQuotaTarget = target.SamplingPointQuotaTargetsQueryAsync("1","1").Result;

            Assert.Equal(expectedSamplingPointQuotaTarget[0].LevelId, actualSamplingPointQuotaTarget.ToArray()[0].LevelId);
            Assert.Equal(expectedSamplingPointQuotaTarget[1].LevelId, actualSamplingPointQuotaTarget.ToArray()[1].LevelId);
            Assert.Equal(2, actualSamplingPointQuotaTarget.Count());
        }

        #endregion

        #region SamplingPointQuotaTargetUpdateAsync

        [Fact]
        public void TestSamplingPointQuotaTargetUpdateAsync_SamplingPointQuotaTargetArgumentIsNull_ThrowsArgumentNullException()
        {
            var target = new NfieldSurveysService();
            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    try
                    {
                        target.SamplingPointQuotaTargetUpdateAsync("", "", null).Wait();
                    }
                    catch (AggregateException ex)
                    {
                        throw ex.InnerException;
                    }
                }
                );
        }

        [Fact]
        public void TestSamplingPointQuotaTargetUpdateAsync_SamplingPointQuotaTargetExists_ReturnsSamplingPointQuotaTarget()
        {
            const string levelId = "LevelId";
            const string surveyId = "SurveyId";
            const string samplingPointId = "SamplingPointId";

            var samplingPointQuotaTarget = new SamplingPointQuotaTarget
            {
                LevelId = levelId,
                Target = 10
            };
            var mockedNfieldConnection = new Mock<INfieldConnectionClient>();
            var mockedHttpClient = CreateHttpClientMock(mockedNfieldConnection);
            mockedHttpClient
                .Setup(
                    client =>
                        client.PatchAsJsonAsync<UpdateSamplingPointQuotaTarget>(
                            string.Format("{0}surveys/{1}/samplingpoints/{2}/quotatargets/{3}", ServiceAddress, surveyId,
                                samplingPointId, levelId), It.IsAny<UpdateSamplingPointQuotaTarget>()))
                .Returns(CreateTask(HttpStatusCode.OK,
                    new StringContent(JsonConvert.SerializeObject(samplingPointQuotaTarget))));

            var target = new NfieldSurveysService();
            target.InitializeNfieldConnection(mockedNfieldConnection.Object);

            var actual = target.SamplingPointQuotaTargetUpdateAsync(surveyId,samplingPointId,samplingPointQuotaTarget).Result;

            Assert.Equal(samplingPointQuotaTarget.Target, actual.Target);
        }

        #endregion
    }
}
