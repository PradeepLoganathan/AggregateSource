﻿using System;
using System.Net;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace AggregateSource.EventStorage {
  [TestFixture]
  public class SampleUsage {
    [Test]
    public void Show() {
      using (var connection = EventStoreConnection.Create()) {
        connection.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1113));
        
        //Dependencies
        var dogRepository = new EventStoreRepository<Dog>(Dog.Factory, connection);
        var dogApplicationServices = new DogApplicationServices(dogRepository);

        //Aggregate identifier
        var dogId = Guid.NewGuid();

        //First command - creates a stream
        var registerBirthOfDog = new RegisterBirthOfDogCommand(
          dogId,
          "Sparky",
          DateTime.Today.AddYears(-1));
        var registerBirthOfDogHandler = 
          new EventStoreAwareHandler<RegisterBirthOfDogCommand>(connection, dogApplicationServices);
        registerBirthOfDogHandler.Handle(registerBirthOfDog);

        //Second command - reads a stream and appends to it
        var registerThatTheDogGotAShot = new RegisterThatTheDogGotAShot(
          dogId, 
          "Cocaine", 
          DateTime.Today);
        var registerThatTheDogGotAShotHandler =
          new EventStoreAwareHandler<RegisterThatTheDogGotAShot>(connection, dogApplicationServices);
        registerThatTheDogGotAShotHandler.Handle(registerThatTheDogGotAShot);  
      }
    }
  }
}