﻿using System;
using SwiftSuspenders;
using NUnit.Framework;
using SwiftSuspenders.Support.Types;
using SwiftSuspenders.DependencyProviders;
using SwiftSuspenders.Errors;
using SwiftSuspenders.Support.providers;
using SwiftSuspenders.Mapping;
using SwiftSuspenders.Support.Injectees;

namespace SwiftSuspenders
{
	[TestFixture]
	public class DependencyProviderTests
	{
		private Injector injector;

		[SetUp()]
		public void Setup()
		{
			injector = new Injector();
		}

		[TearDown()]
		public void Teardown()
		{
			Injector.PurgeInjectionPointsCache();
			injector = null;
		}

		[Test]
		public void ValueProviderReturnsSetValue()
		{
			Clazz response = new Clazz();
			ValueProvider provider = new ValueProvider(response);
			object returnedResponse = provider.Apply(null, injector, null);

			Assert.AreEqual(response, returnedResponse);
		}

		[Test]
		public void TypeProviderReturnsClassInstance()
		{
			TypeProvider classProvider = new TypeProvider(typeof(Clazz));
			object returnedResponse = classProvider.Apply(null, injector, null);

			Assert.IsInstanceOf<Clazz>(returnedResponse);
		}

		[Test]
		public void TypeProviderReturnsDifferentInstancesOnEachApply()
		{
			TypeProvider classProvider = new TypeProvider(typeof(Clazz));
			object firstResponse = classProvider.Apply(null, injector, null);
			object secondResponse = classProvider.Apply(null, injector, null);

			Assert.AreNotEqual(firstResponse, secondResponse);
		}

		[Test]
		public void SingletonProviderReturnsInstance()
		{
			SingletonProvider singletonProvider = new SingletonProvider(typeof(Clazz), injector);
			object returnedResponse = singletonProvider.Apply(null, injector, null);

			Assert.IsInstanceOf<Clazz>(returnedResponse);
		}

		[Test]
		public void SameSingletonIsReturnedOnSecondResponse()
		{
			SingletonProvider singletonProvider = new SingletonProvider(typeof(Clazz), injector);
			object returnedResponse = singletonProvider.Apply(null, injector, null);
			object secondResponse = singletonProvider.Apply(null, injector, null);

			Assert.AreEqual(returnedResponse, secondResponse);
		}

		[Test]
		public void DestroyingSingletonProviderInvokesPreDestroyMethodsOnSingleton()
		{
			SingletonProvider provider = new SingletonProvider(typeof(Clazz), injector);
			Clazz singleton = provider.Apply(null, injector, null) as Clazz;
			provider.Destroy();
			Assert.True (singleton.preDestroyCalled);
		}

		[Test, ExpectedException(typeof(InjectorException))]
		public void usingDestroyedSingletonProviderThrows()
		{
			SingletonProvider provider = new SingletonProvider(typeof(Clazz), injector);
			provider.Destroy();
			provider.Apply(null, injector, null);
		}

		[Test]
		public void MappingToProviderUsesProvidersResponse()
		{
			InjectionMapping otherConfig = new InjectionMapping(injector, new MappingId(typeof(ClazzExtension), null));
			otherConfig.ToProvider(new TypeProvider(typeof(ClazzExtension)));
			OtherMappingProvider otherMappingProvider = new OtherMappingProvider(otherConfig);
			object returnedResponse = otherMappingProvider.Apply(null, injector, null);

			Assert.IsInstanceOf<Clazz>(returnedResponse);
			Assert.IsInstanceOf<ClazzExtension>(returnedResponse);
		}

		[Test]
		public void DependencyProviderHasAccessToTargetType()
		{
			ClassNameStoringProvider provider = new ClassNameStoringProvider();
			injector.Map(typeof(Clazz)).ToProvider(provider);
			injector.InstantiateUnmapped(typeof(ClassInjectee));

			Assert.AreEqual(typeof(ClassInjectee).AssemblyQualifiedName, provider.lastTargetClassName, "ClassName stored in provider is not AssemblyQualifiedName of ClassInjectee");
		}
	}
}

