﻿using System;
using System.IO;
using AggregateSource.GEventStore.Framework;
using AggregateSource.GEventStore.Snapshots.Framework;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace AggregateSource.GEventStore.Snapshots {
  namespace AsyncSnapshotReaderTests {
    [TestFixture]
    public class WithAnyInstance {
      SnapshotStoreReadConfiguration _configuration;
      EventStoreConnection _connection;

      [SetUp]
      public void SetUp() {
        _configuration = SnapshotStoreReadConfigurationFactory.Create();
        _connection = EmbeddedEventStore.Instance.Connection;
      }

      [Test]
      public void ConnectionCannotBeNull() {
        Assert.Throws<ArgumentNullException>(() => new AsyncSnapshotReader(null, _configuration));
      }

      [Test]
      public void ConfigurationCannotBeNull() {
        Assert.Throws<ArgumentNullException>(() => new AsyncSnapshotReader(_connection, null));
      }

      [Test]
      public void IsAsyncSnapshotReader() {
        Assert.That(AsyncSnapshotReaderFactory.Create(), Is.InstanceOf<IAsyncSnapshotReader>());
      }

      [Test]
      public void ConfigurationReturnsExpectedValue() {
        var configuration = SnapshotStoreReadConfigurationFactory.Create();
        Assert.That(AsyncSnapshotReaderFactory.CreateWithConfiguration(configuration).Configuration, Is.SameAs(configuration));
      }

      [Test]
      public void ConnectionReturnsExpectedValue() {
        var connection = EmbeddedEventStore.Instance.Connection;
        Assert.That(AsyncSnapshotReaderFactory.CreateWithConnection(connection).Connection, Is.SameAs(connection));
      }

      [Test]
      public void ReadIdentifierCannotBeNull() {
        var sut = AsyncSnapshotReaderFactory.Create();
        var exception = Assert.Throws<AggregateException>(() => { var _ = sut.ReadOptionalAsync(null).Result; });
        Assert.That(exception.InnerExceptions, Has.Count.EqualTo(1));
        Assert.That(exception.InnerExceptions[0], Is.InstanceOf<ArgumentNullException>());
      }
    }

    [TestFixture]
    public class WithSnapshotStreamFoundInStore {
      Model _model;
      AsyncSnapshotReader _sut;

      [SetUp]
      public void SetUp() {
        _model = new Model();
        _sut = AsyncSnapshotReaderFactory.Create();
        CreateSnapshotStreamWithOneSnapshot(_sut.Configuration.Resolver.Resolve(_model.KnownIdentifier));
      }

      static void CreateSnapshotStreamWithOneSnapshot(string snapshotStreamName) {
        using (var stream = new MemoryStream()) {
          using (var writer = new BinaryWriter(stream)) {
            new SnapshotState().Write(writer);
          }
          EmbeddedEventStore.Instance.Connection.AppendToStream(
            snapshotStreamName,
            ExpectedVersion.NoStream,
            new EventData(
              Guid.NewGuid(),
              typeof(SnapshotState).AssemblyQualifiedName,
              false,
              stream.ToArray(),
              BitConverter.GetBytes(100)));
        }
      }

      [Test]
      public void GetReturnsSnapshotOfKnownId() {
        var result = _sut.ReadOptionalAsync(_model.KnownIdentifier).Result;

        Assert.That(result, Is.EqualTo(new Optional<Snapshot>(new Snapshot(100, new SnapshotState()))));
      }

      [Test]
      public void GetReturnsEmptyForUnknownId() {
        var result = _sut.ReadOptionalAsync(_model.UnknownIdentifier).Result;

        Assert.That(result, Is.EqualTo(Optional<Snapshot>.Empty));
      }
    }

    [TestFixture]
    public class WithSnapshotStreamNotFoundInStore {
      Model _model;
      AsyncSnapshotReader _sut;

      [SetUp]
      public void SetUp() {
        _model = new Model();
        _sut = AsyncSnapshotReaderFactory.Create();
      }

      [Test]
      public void GetReturnsEmptyForKnownId() {
        var result = _sut.ReadOptionalAsync(_model.KnownIdentifier).Result;

        Assert.That(result, Is.EqualTo(Optional<Snapshot>.Empty));
      }
    }

    [TestFixture]
    public class WithEmptySnapshotStreamInStore {
      Model _model;
      AsyncSnapshotReader _sut;

      [SetUp]
      public void SetUp() {
        _model = new Model();
        _sut = AsyncSnapshotReaderFactory.Create();
        CreateEmptySnapshotStream(_sut.Configuration.Resolver.Resolve(_model.KnownIdentifier));
      }

      static void CreateEmptySnapshotStream(string snapshotStreamName) {
        EmbeddedEventStore.Instance.Connection.CreateStream(
          snapshotStreamName,
          Guid.NewGuid(),
          false,
          new byte[0]);
      }

      [Test]
      public void GetReturnsEmptyForKnownId() {
        var result = _sut.ReadOptionalAsync(_model.KnownIdentifier).Result;

        Assert.That(result, Is.EqualTo(Optional<Snapshot>.Empty));
      }

      [Test]
      public void GetReturnsEmptyForUnknownId() {
        var result = _sut.ReadOptionalAsync(_model.UnknownIdentifier).Result;

        Assert.That(result, Is.EqualTo(Optional<Snapshot>.Empty));
      }
    }

    [TestFixture]
    public class WithDeletedSnapshotStreamInStore {
      Model _model;
      AsyncSnapshotReader _sut;

      [SetUp]
      public void SetUp() {
        _model = new Model();
        _sut = AsyncSnapshotReaderFactory.Create();
        CreateDeletedSnapshotStream(_sut.Configuration.Resolver.Resolve(_model.KnownIdentifier));
      }

      static void CreateDeletedSnapshotStream(string snapshotStreamName) {
        EmbeddedEventStore.Instance.Connection.CreateStream(
          snapshotStreamName,
          Guid.NewGuid(),
          false,
          new byte[0]);
        EmbeddedEventStore.Instance.Connection.DeleteStream(
          snapshotStreamName,
          ExpectedVersion.EmptyStream);
      }

      [Test]
      public void GetReturnsSnapshotOfKnownId() {
        var result = _sut.ReadOptionalAsync(_model.KnownIdentifier).Result;

        Assert.That(result, Is.EqualTo(Optional<Snapshot>.Empty));
      }

      [Test]
      public void GetReturnsEmptyForUnknownId() {
        var result = _sut.ReadOptionalAsync(_model.UnknownIdentifier).Result;

        Assert.That(result, Is.EqualTo(Optional<Snapshot>.Empty));
      }
    }
  }
}
