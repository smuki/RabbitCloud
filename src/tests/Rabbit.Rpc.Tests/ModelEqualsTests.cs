﻿using Rabbit.Rpc.Address;
using Rabbit.Rpc.Routing;
using Rabbit.Rpc.Runtime.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Rabbit.Rpc.Tests
{
    public class ModelEqualsTests
    {
        [Fact]
        public void IpAddressModelEqualsTest()
        {
            AddressModel model1 = new IpAddressModel("127.0.0.1", 1234);
            AddressModel model2 = new IpAddressModel("127.0.0.1", 1234);
            AddressModel model3 = new IpAddressModel("127.0.0.1", 12345);
            AddressModel model4 = new IpAddressModel("127.0.0.2", 1234);
            AddressModel model5 = new IpAddressModel("127.0.0.2", 12345);

            Assert.Equal(model1, model2);
            Assert.True(model1.Equals(model2));
            Assert.True(model1 == model2);

            Assert.False(!model1.Equals(model2));
            Assert.False(model1 != model2);

            Assert.False(model1.Equals(model3));
            Assert.False(model1 == model3);
            Assert.False(model1.Equals(model4));
            Assert.False(model1 == model4);
            Assert.False(model1.Equals(model5));
            Assert.False(model1 == model5);

            Assert.NotEqual(model1, model3);
            Assert.True(!model1.Equals(model3));
            Assert.True(model1 != model3);
            Assert.True(!model1.Equals(model4));
            Assert.True(model1 != model4);
            Assert.True(!model1.Equals(model5));
            Assert.True(model1 != model5);

            var array1 = new[] { new IpAddressModel("127.0.0.1", 1234), new IpAddressModel("127.0.0.2", 1234) };
            var array2 = new[] { new IpAddressModel("127.0.0.1", 1234), new IpAddressModel("127.0.0.2", 1234) };

            Assert.False(array1.Except(array2).Any());
            Assert.Equal(2, array1.Intersect(array2).Count());
        }

        [Fact]
        public void ServiceDescriptorEqualsTest()
        {
            ServiceRecord model1 = null, model2 = null;
            Action reset = () =>
            {
                model1 = new ServiceRecord
                {
                    ServiceName = "service1",
                    Metadata = new Dictionary<string, object>
                    {
                        {"key1", 1}
                    }
                };
                model2 = new ServiceRecord
                {
                    ServiceName = "service1",
                    Metadata = new Dictionary<string, object>
                    {
                        {"key1", 1}
                    }
                };
            };
            reset();

            Assert.Equal(model1, model2);
            Assert.True(model1 == model2);
            Assert.True(model1.Equals(model2));

            model2.ServiceName = "service2";

            Assert.NotEqual(model1, model2);
            Assert.False(model1 == model2);
            Assert.False(model1.Equals(model2));

            reset();
            model2.Metadata["temp"] = 2;

            Assert.NotEqual(model1, model2);
            Assert.False(model1 == model2);
            Assert.False(model1.Equals(model2));

            model1.Metadata["temp"] = 2;

            Assert.Equal(model1, model2);
            Assert.True(model1 == model2);
            Assert.True(model1.Equals(model2));
        }

        [Fact]
        public void ServiceRouteEqualsTest()
        {
            ServicePath model1 = null, model2 = null;
            Action reset = () =>
            {
                model1 =
                    new ServicePath
                    {
                        Address = new[]
                        {
                            new IpAddressModel("127.0.0.1", 1234)
                        },
                        ServiceEntry = new ServiceRecord
                        {
                            ServiceName = "service1",
                            Metadata = new Dictionary<string, object>
                            {
                                {"key1", 1}
                            }
                        }
                    };
                model2 =
                    new ServicePath
                    {
                        Address = new[]
                        {
                            new IpAddressModel("127.0.0.1", 1234)
                        },
                        ServiceEntry = new ServiceRecord
                        {
                            ServiceName = "service1",
                            Metadata = new Dictionary<string, object>
                            {
                                {"key1", 1}
                            }
                        }
                    };
            };
            reset();

            Assert.Equal(model1, model2);
            Assert.True(model1 == model2);
            Assert.True(model1.Equals(model2));

            model2.ServiceEntry.ServiceName = "service2";

            Assert.NotEqual(model1, model2);
            Assert.False(model1 == model2);
            Assert.False(model1.Equals(model2));

            model1.ServiceEntry.ServiceName = "service2";

            Assert.Equal(model1, model2);
            Assert.True(model1 == model2);
            Assert.True(model1.Equals(model2));

            reset();

            model2.Address.OfType<IpAddressModel>().First().Port = 1111;

            Assert.NotEqual(model1, model2);
            Assert.False(model1 == model2);
            Assert.False(model1.Equals(model2));
        }
    }
}