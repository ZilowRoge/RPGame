using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RPGame.Core.Damage;
using UnityEngine;

namespace RPGame.Enemies.Tests
{
    public sealed class ProjectileLaunchDataTests
    {
        [Test]
        public void ProjectileLaunchData_ContainsOnlySingleLaunchData()
        {
            PropertyInfo[] properties = typeof(ProjectileLaunchData)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public);

            Assert.IsTrue(properties.Any(property => property.Name == "ProjectilePrefab" && property.PropertyType == typeof(GameObject)));
            Assert.IsTrue(properties.Any(property => property.Name == "TargetPosition" && property.PropertyType == typeof(Vector3)));
            Assert.IsTrue(properties.Any(property => property.Name == "TargetDamageable" && property.PropertyType == typeof(IDamageable)));
            Assert.IsTrue(properties.Any(property => property.Name == "DamageParts"));
            Assert.IsTrue(properties.Any(property => property.Name == "Source" && property.PropertyType == typeof(GameObject)));
        }

        [Test]
        public void ProjectileLaunchData_DoesNotContainTrajectoryOrMovementData()
        {
            string[] forbiddenNames =
            {
                "ProjectileSpeed",
                "ProjectileLifetime",
                "ArcHeight",
                "AscentDuration",
                "DescentDuration",
                "ParabolicProjectilePrefab"
            };

            PropertyInfo[] properties = typeof(ProjectileLaunchData)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (string forbiddenName in forbiddenNames)
            {
                Assert.IsFalse(properties.Any(property => property.Name == forbiddenName), forbiddenName);
            }
        }
    }
}
