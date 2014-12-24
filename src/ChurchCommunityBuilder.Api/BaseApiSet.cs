﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;

namespace ChurchCommunityBuilder.Api {
    public class BaseApiSet<T> where T : new() {
        private readonly string _baseUrl;
        private string _username;
        private string _password;
        private readonly ContentType _contentType;
        private IDictionary<string, string> _parameters = new Dictionary<string, string>();

        public string BaseUrl { get { return _baseUrl; } }
        private string _serviceName;
        protected virtual string ServiceName {
            get {
                if (string.IsNullOrEmpty(_serviceName)) {
                    throw new NotImplementedException("The property ServiceName has no value on the BaseApiSet.");
                }
                return _serviceName;
            }
            set { this._serviceName = value; }
        }
            
        public BaseApiSet(string baseUrl, string username, string password) {
            _baseUrl = baseUrl;
            _username = username;
            _password = password;
        }

        public S Execute<S>(QueryObject qo, string serviceName) where S : new() {
            this._parameters.Add("srv", serviceName);
            var request = CreateRestRequest(Method.GET, _baseUrl);

            foreach (var pair in qo.ToDictionary()) {
                request.AddParameter(pair.Key, pair.Value);
            }

            var results = ExecuteGenericRequest<S>(request);
            return results.Data;
        }

        protected IRestResponse<S> ExecuteGenericRequest<S>(IRestRequest request) where S : new() {
            var client = new RestClient(_baseUrl);
            client.Authenticator = new HttpBasicAuthenticator(this._username, this._password);
            var response = client.Execute<S>(request);

            if ((int)response.StatusCode > 300) {
                throw new Exception(response.StatusDescription);
            }

            return response;
        }

        private RestRequest CreateRestRequest(Method method, string url, string contentType = null) {

            var request = new RestRequest(method) {
                Resource = url
            };
            request.RequestFormat = _contentType == ContentType.JSON ? DataFormat.Json : DataFormat.Xml;
            request.AddHeader("Accept-Encoding", "gzip,deflate");
            request.AddHeader("Content-Type", !string.IsNullOrEmpty(contentType) ? contentType : _contentType == ContentType.XML ? "application/xml" : "application/json");

            if (_parameters != null && _parameters.Count > 0) {
                foreach (var current in _parameters) {
                    request.AddParameter(current.Key, current.Value);
                }
            }

            return request;
        }
    }
}
