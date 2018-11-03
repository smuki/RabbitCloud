//using Rabbit.Rpc.Address;
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
        public void ServiceDescriptorEqualsTest()
        {
            ServiceRecord model1 = null, model2 = null;
            Action reset = () =>
            {
                model1 = new ServiceRecord
                {
                    ServiceId = "service1",
                    Metadata = new Dictionary<string, object>
                    {
                        {"key1", 1}
                    }
                };
                model2 = new ServiceRecord
                {
                    ServiceId = "service1",
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

            model2.ServiceId = "service2";

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
            ServiceRoute model1 = null, model2 = null;
            Action reset = () =>
            {
                model1 =
                    new ServiceRoute
                    {
                        Address = new[]
                        {
                            "127.0.0.1:1234"
                        },
                        ServiceEntry = new ServiceRecord
                        {
                            ServiceId = "service1",
                            Metadata = new Dictionary<string, object>
                            {
                                {"key1", 1}
                            }
                        }
                    };
                model2 =
                    new ServiceRoute
                    {
                        Address = new[]
                        {
                            "127.0.0.1:1234",
                        },
                        ServiceEntry = new ServiceRecord
                        {
                            ServiceId = "service1",
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

            model2.ServiceEntry.ServiceId = "service2";

            Assert.NotEqual(model1, model2);
            Assert.False(model1 == model2);
            Assert.False(model1.Equals(model2));

            model1.ServiceEntry.ServiceId = "service2";

            Assert.Equal(model1, model2);
            Assert.True(model1 == model2);
            Assert.True(model1.Equals(model2));

            reset();

           // model2.Address<string>().First() = "123";

           // Assert.NotEqual(model1, model2);
           // Assert.False(model1 == model2);
           // Assert.False(model1.Equals(model2));
        }
    }
}