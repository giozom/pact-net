﻿using System;
using System.Threading;
using Nancy;
using Nancy.Routing;
using NSubstitute;
using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Comparers;
using PactNet.Mocks.MockHttpService.Mappers;
using PactNet.Mocks.MockHttpService.Models;
using Xunit;

namespace PactNet.Tests.Mocks.MockHttpService
{
    public class MockProviderNancyRequestDispatcherTests : IDisposable
    {
        public void Dispose()
        {
            MockProviderNancyRequestDispatcher.Reset();
        }

        [Fact]
        public void Dispatch_WhenExpectedRequestHasNotBeenSet_ResponseMapperIsCalledAndReturnsInternalServerErrorResponse()
        {
            var nancyContext = new NancyContext
            {
                Request = new Request("GET", "/", "HTTP")
            };

            MockProviderNancyRequestDispatcher.Set(null, new PactProviderServiceResponse());

            var mockNancyResponseMapper = Substitute.For<INancyResponseMapper>();
            mockNancyResponseMapper.Convert(Arg.Any<PactProviderServiceResponse>())
                .Returns(new Response
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            IRequestDispatcher requestDispatcher = new MockProviderNancyRequestDispatcher(null, null, mockNancyResponseMapper);

            var result = requestDispatcher.Dispatch(nancyContext, CancellationToken.None).Result;

            mockNancyResponseMapper.Received(1).Convert(Arg.Is<PactProviderServiceResponse>(x => x.Status == System.Net.HttpStatusCode.InternalServerError));
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [Fact]
        public void Dispatch_WhenExpectedResponseHasNotBeenSet_ResponseMapperIsCalledAndReturnsInternalServerErrorResponse()
        {
            var nancyContext = new NancyContext
            {
                Request = new Request("GET", "/", "HTTP")
            };

            MockProviderNancyRequestDispatcher.Set(new PactProviderServiceRequest(), null);

            var mockNancyResponseMapper = Substitute.For<INancyResponseMapper>();
            mockNancyResponseMapper.Convert(Arg.Any<PactProviderServiceResponse>())
                .Returns(new Response
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            IRequestDispatcher requestDispatcher = new MockProviderNancyRequestDispatcher(null, null, mockNancyResponseMapper);

            var result = requestDispatcher.Dispatch(nancyContext, CancellationToken.None).Result;

            mockNancyResponseMapper.Received(1).Convert(Arg.Is<PactProviderServiceResponse>(x => x.Status == System.Net.HttpStatusCode.InternalServerError));
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [Fact]
        public void Dispatch_WithCanceledCancellationToken_ResponseMapperIsCalledAndReturnsInternalServerErrorResponse()
        {
            var nancyContext = new NancyContext
            {
                Request = new Request("GET", "/", "HTTP")
            };

            MockProviderNancyRequestDispatcher.Set(new PactProviderServiceRequest(), new PactProviderServiceResponse());

            var mockNancyResponseMapper = Substitute.For<INancyResponseMapper>();
            mockNancyResponseMapper.Convert(Arg.Any<PactProviderServiceResponse>())
                .Returns(new Response
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            IRequestDispatcher requestDispatcher = new MockProviderNancyRequestDispatcher(null, null, mockNancyResponseMapper);

            var result = requestDispatcher.Dispatch(nancyContext, CancellationToken.None).Result;

            mockNancyResponseMapper.Received(1).Convert(Arg.Is<PactProviderServiceResponse>(x => x.Status == System.Net.HttpStatusCode.InternalServerError));
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [Fact]
        public void Dispatch_WithNancyContext_ConvertIsCalledOnThePactProviderServiceRequestMapper()
        {
            var nancyContext = new NancyContext
            {
                Request = new Request("GET", "/", "HTTP")
            };
            var expectedRequest = new PactProviderServiceRequest();
            var expectedResponse = new PactProviderServiceResponse();

            var mockRequestComparer = Substitute.For<IPactProviderServiceRequestComparer>();
            var mockRequestMapper = Substitute.For<IPactProviderServiceRequestMapper>();
            var mockResponseMapper = Substitute.For<INancyResponseMapper>();

            MockProviderNancyRequestDispatcher.Set(expectedRequest, expectedResponse);

            IRequestDispatcher requestDispatcher = new MockProviderNancyRequestDispatcher(
                mockRequestComparer,
                mockRequestMapper,
                mockResponseMapper);

            requestDispatcher.Dispatch(nancyContext, CancellationToken.None);

            mockRequestMapper.Received(1).Convert(nancyContext.Request);
        }

        [Fact]
        public void Dispatch_WithNancyContext_CompareIsCalledOnThePactProviderServiceRequestComparer()
        {
            var nancyContext = new NancyContext
            {
                Request = new Request("GET", "/", "HTTP")
            };
            var expectedRequest = new PactProviderServiceRequest();
            var expectedResponse = new PactProviderServiceResponse();
            var actualRequest = new PactProviderServiceRequest();

            var mockRequestComparer = Substitute.For<IPactProviderServiceRequestComparer>();
            var mockRequestMapper = Substitute.For<IPactProviderServiceRequestMapper>();
            var mockResponseMapper = Substitute.For<INancyResponseMapper>();

            MockProviderNancyRequestDispatcher.Set(expectedRequest, expectedResponse);

            mockRequestMapper.Convert(nancyContext.Request).Returns(actualRequest);

            IRequestDispatcher requestDispatcher = new MockProviderNancyRequestDispatcher(
                mockRequestComparer,
                mockRequestMapper,
                mockResponseMapper);

            requestDispatcher.Dispatch(nancyContext, CancellationToken.None);

            mockRequestComparer.Received(1).Compare(expectedRequest, actualRequest);
        }

        [Fact]
        public void Dispatch_WithNancyContext_ConvertIsCalledOnTheNancyResponseMapper()
        {
            var nancyContext = new NancyContext
            {
                Request = new Request("GET", "/", "HTTP")
            };
            var expectedRequest = new PactProviderServiceRequest();
            var expectedResponse = new PactProviderServiceResponse();

            var mockRequestComparer = Substitute.For<IPactProviderServiceRequestComparer>();
            var mockRequestMapper = Substitute.For<IPactProviderServiceRequestMapper>();
            var mockResponseMapper = Substitute.For<INancyResponseMapper>();

            MockProviderNancyRequestDispatcher.Set(expectedRequest, expectedResponse);

            IRequestDispatcher requestDispatcher = new MockProviderNancyRequestDispatcher(
                mockRequestComparer,
                mockRequestMapper,
                mockResponseMapper);

            requestDispatcher.Dispatch(nancyContext, CancellationToken.None);

            mockResponseMapper.Received(1).Convert(expectedResponse);
        }

        [Fact]
        public void Dispatch_WithNancyContextRequestThatMatchesExpectedRequest_ReturnsNancyResponse()
        {
            var nancyContext = new NancyContext
            {
                Request = new Request("GET", "/Test", "HTTP")
            };
            var expectedRequest = new PactProviderServiceRequest
            {
                Method = HttpVerb.Get,
                Path = "/Test"
            };
            var actualRequest = new PactProviderServiceRequest
            {
                Method = HttpVerb.Get,
                Path = "/Test"
            };
            var expectedResponse = new PactProviderServiceResponse { Status = System.Net.HttpStatusCode.OK };
            var nancyResponse = new Response { StatusCode = HttpStatusCode.OK };

            var mockRequestComparer = Substitute.For<IPactProviderServiceRequestComparer>();
            var mockRequestMapper = Substitute.For<IPactProviderServiceRequestMapper>();
            var mockResponseMapper = Substitute.For<INancyResponseMapper>();

            MockProviderNancyRequestDispatcher.Set(expectedRequest, expectedResponse);

            mockRequestMapper.Convert(nancyContext.Request).Returns(actualRequest);
            //mockRequestComparer.Compare Doesnt throw any exceptions
            mockResponseMapper.Convert(expectedResponse).Returns(nancyResponse);

            IRequestDispatcher requestDispatcher = new MockProviderNancyRequestDispatcher(
                mockRequestComparer,
                mockRequestMapper,
                mockResponseMapper);

            var response = requestDispatcher.Dispatch(nancyContext, CancellationToken.None);

            Assert.Equal(nancyResponse, response.Result);
            Assert.Equal(nancyResponse, nancyContext.Response);
            Assert.Null(response.Exception);
        }

        [Fact]
        public void Dispatch_WithNancyContextRequestThatDoesNotMatchExpectedRequest_ResponseMapperIsCalledAndReturnsInternalServerErrorResponse()
        {
            var nancyContext = new NancyContext
            {
                Request = new Request("GET", "/Test", "HTTP")
            };
            var expectedRequest = new PactProviderServiceRequest
            {
                Method = HttpVerb.Get,
                Path = "/Test"
            };
            var actualRequest = new PactProviderServiceRequest
            {
                Method = HttpVerb.Put,
                Path = "/Test"
            };
            var expectedResponse = new PactProviderServiceResponse { Status = System.Net.HttpStatusCode.OK };
            var nancyResponse = new Response { StatusCode = HttpStatusCode.OK };
            var compareException = new CompareFailedException(expectedRequest.Method, actualRequest.Method);

            var mockRequestComparer = Substitute.For<IPactProviderServiceRequestComparer>();
            var mockRequestMapper = Substitute.For<IPactProviderServiceRequestMapper>();
            var mockResponseMapper = Substitute.For<INancyResponseMapper>();

            MockProviderNancyRequestDispatcher.Set(expectedRequest, expectedResponse);

            mockRequestMapper.Convert(nancyContext.Request).Returns(actualRequest);
            mockRequestComparer
                .When(x => x.Compare(expectedRequest, actualRequest))
                .Do(x => { throw compareException; });

            mockResponseMapper.Convert(expectedResponse).Returns(nancyResponse);

            mockResponseMapper.Convert(Arg.Any<PactProviderServiceResponse>())
                .Returns(new Response
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            IRequestDispatcher requestDispatcher = new MockProviderNancyRequestDispatcher(
                mockRequestComparer,
                mockRequestMapper,
                mockResponseMapper);

            var response = requestDispatcher.Dispatch(nancyContext, CancellationToken.None).Result;

            mockResponseMapper.Received(1).Convert(Arg.Is<PactProviderServiceResponse>(x => x.Status == System.Net.HttpStatusCode.InternalServerError));
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }
    }
}
