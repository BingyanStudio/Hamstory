using System;
using System.Text;
using Bingyan;
using UnityEngine;

namespace Hamstory
{
    [CreateAssetMenu(fileName = "ArchiveDataProvider", menuName = "Hamstory/Data/Archive")]
    public class ArchiveDataProvider : BasicDataProvider
    {
        protected override object Get(string key) => Archive.Get<object>(key, null);
    }
}