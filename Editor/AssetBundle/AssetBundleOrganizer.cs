using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleOrganizer
    {
        /// <summary>
        /// Asset bundle path segment regular expression.
        /// </summary>
        /// <remarks>Only lower case letters, digits and underscore are allowed.</remarks>
        public static readonly Regex AssetBundlePathSegmentRegex = new Regex(@"^[\w\d_]+$");

        private const string DefaultConfigPath = "Assets/AssetBundleOrganizerConfig.xml";

        private static string s_ConfigPath = null;

        public static string ConfigPath =>
            s_ConfigPath ?? (s_ConfigPath = Utility.Config.Read<AssetBundleOrganizerConfigPathAttribute, string>() ?? DefaultConfigPath);

        private AssetBundleOrganizerConfig m_Config = new AssetBundleOrganizerConfig();
        private AssetBundleOrganizerConfigCache m_ConfigCache = null;

        public AssetBundleOrganizerConfigCache ConfigCache => m_ConfigCache;

        private readonly List<AssetInfo> m_AssetInfoForestRoots = new List<AssetInfo>();

        public IList<AssetInfo> AssetInfoForestRoots => m_AssetInfoForestRoots.AsReadOnly();

        private readonly AssetBundleInfo m_AssetBundleInfoTreeRoot = new AssetBundleInfo {IsDirectory = true, Parent = null, Path = string.Empty};

        public AssetBundleInfo AssetBundleInfoTreeRoot => m_AssetBundleInfoTreeRoot;

        private readonly Dictionary<string, AssetInfo> m_IncludedAssetGuidToInfoMap = new Dictionary<string, AssetInfo>();
        private readonly XmlSerializer m_ConfigSerializer = new XmlSerializer(typeof(AssetBundleOrganizerConfig));

        public AssetBundleOrganizer()
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                using (var sw = new StreamWriter(ConfigPath))
                {
                    m_ConfigSerializer.Serialize(sw, m_Config);
                }

                AssetDatabase.ImportAsset(ConfigPath);
            }

            using (var sr = new StreamReader(ConfigPath))
            {
                m_Config = (AssetBundleOrganizerConfig)m_ConfigSerializer.Deserialize(sr);
            }

            m_ConfigCache = new AssetBundleOrganizerConfigCache(m_Config);
            m_ConfigCache.SyncToCache();
        }

        public void SaveConfig()
        {
            NormalizeRootAssetDirectories();
            m_ConfigCache.SyncFromCache();

            using (var sw = new StreamWriter(ConfigPath))
            {
                m_ConfigSerializer.Serialize(sw, m_Config);
            }

            AssetDatabase.ImportAsset(ConfigPath);
        }

        public void NormalizeRootAssetDirectories()
        {
            var pairs = m_ConfigCache.RootDirectoryInfos;
            pairs.Sort(CompareRootAssetDirectories);

            int normalizedCount = 0;
            foreach (var pair in pairs)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(pair.DirectoryGuid);
                if (pair.DirectoryGuid == null || AssetDatabase.LoadAssetAtPath<Object>(assetPath) == null)
                {
                    continue;
                }

                if (Utility.Path.IsEditor(assetPath))
                {
                    continue;
                }

                if (normalizedCount > 0 && pair.DirectoryGuid == pairs[normalizedCount - 1].DirectoryGuid)
                {
                    if (!string.IsNullOrEmpty(pair.Filter.Trim()))
                    {
                        pairs[normalizedCount - 1].Filter =
                            string.Join(" ", pairs[normalizedCount - 1].Filter, pair.Filter).Trim();
                    }
                }
                else
                {
                    pairs[normalizedCount].DirectoryGuid = pair.DirectoryGuid;
                    pairs[normalizedCount].Filter = pair.Filter.Trim();
                    normalizedCount++;
                }
            }

            pairs.RemoveRange(normalizedCount, pairs.Count - normalizedCount);
        }

        public void RefreshAssetForest()
        {
            NormalizeRootAssetDirectories();
            m_AssetInfoForestRoots.Clear();
            m_IncludedAssetGuidToInfoMap.Clear();
            foreach (var pair in m_ConfigCache.RootDirectoryInfos)
            {
                BuildAssetTree(pair);
            }

            //LogAssetInfoForest();
        }

        public void RefreshAssetBundleTree()
        {
            m_AssetBundleInfoTreeRoot.Children.Clear();

            foreach (var rawData in m_ConfigCache.AssetBundleInfos.Values)
            {
                var relPathToRoot = Regex.Replace(rawData.AssetBundlePath, @"^" + m_AssetBundleInfoTreeRoot.Path, string.Empty);
                var segments = relPathToRoot.Split('/');
                var currentRelPath = string.Empty;
                var node = m_AssetBundleInfoTreeRoot;
                for (int i = 0; i < segments.Length; i++)
                {
                    var segment = segments[i];
                    currentRelPath += currentRelPath == string.Empty ? segment : ("/" + segment);

                    if (node.Children.ContainsKey(segment))
                    {
                        node = node.Children[segment];
                    }
                    else
                    {
                        var newAssetPath = m_AssetBundleInfoTreeRoot.Path + currentRelPath;
                        var newNode = new AssetBundleInfo
                        {
                            Path = newAssetPath,
                            Name = segment,
                            IsDirectory = i < segments.Length - 1,
                            Parent = node,
                            GroupId = rawData.AssetBundleGroup,
                            DontPack = rawData.DontPack,
                        };

                        node.Children.Add(segment, newNode);
                        node = newNode;
                    }
                }
            }

            //LogAssetBundleInfoTree(m_AssetBundleInfoTreeRoot);
        }

        public void RegroupAssetBundle(string assetBundlePath, int assetBundleGroup)
        {
            var assetBundleInfo = GetAssetBundleInfo(assetBundlePath);
            if (assetBundleInfo == null)
            {
                throw new ArgumentException(Core.Utility.Text.Format("Cannot find asset bundle with path '{0}'.", assetBundlePath));
            }

            if (assetBundleGroup < Core.Asset.Constant.CommonResourceGroupId)
            {
                throw new ArgumentOutOfRangeException(nameof(assetBundleGroup), "Must be non-negative.");
            }

            if (assetBundleInfo.GroupId == assetBundleGroup)
            {
                return;
            }

            assetBundleInfo.GroupId = assetBundleGroup;
            m_ConfigCache.AssetBundleInfos[assetBundlePath].AssetBundleGroup = assetBundleGroup;
        }

        public void SetAssetBundleDontPack(string assetBundlePath, bool dontPack)
        {
            var assetBundleInfo = GetAssetBundleInfo(assetBundlePath);
            if (assetBundleInfo == null)
            {
                throw new ArgumentException(Core.Utility.Text.Format("Cannot find asset bundle with path '{0}'.", assetBundlePath));
            }

            if (assetBundleInfo.DontPack == dontPack)
            {
                return;
            }

            assetBundleInfo.DontPack = dontPack;
            m_ConfigCache.AssetBundleInfos[assetBundlePath].DontPack = dontPack;
        }

        public void RenameAssetBundle(string oldAssetBundlePath, string newAssetBundlePath)
        {
            if (oldAssetBundlePath == newAssetBundlePath)
            {
                return;
            }

            if (!AssetBundlePathIsAvailable(newAssetBundlePath))
            {
                throw new ArgumentException(Core.Utility.Text.Format("Already has an asset bundle with path '{0}'",
                    newAssetBundlePath));
            }

            var assetBundleInfo = GetAssetBundleInfo(oldAssetBundlePath);
            RemoveAssetBundleInfoFromTree(assetBundleInfo);
            assetBundleInfo.Name = newAssetBundlePath.Substring(newAssetBundlePath.LastIndexOf('/') + 1);
            assetBundleInfo.Path = newAssetBundlePath;
            AddAssetBundleInfoIntoTree(assetBundleInfo);

            var rawABInfo = m_ConfigCache.AssetBundleInfos[oldAssetBundlePath];
            m_ConfigCache.AssetBundleInfos.Remove(oldAssetBundlePath);
            rawABInfo.AssetBundlePath = newAssetBundlePath;
            m_ConfigCache.AssetBundleInfos[newAssetBundlePath] = rawABInfo;

            RefreshAssetBundleNamesInAssetForest(oldAssetBundlePath, newAssetBundlePath);
        }

        public AssetBundleInfo CreateNewAssetBundle(string assetBundlePath, int assetBundleGroup, bool DontPack)
        {
            if (assetBundleGroup < Core.Asset.Constant.CommonResourceGroupId)
            {
                throw new ArgumentOutOfRangeException(nameof(assetBundleGroup), "Must be non-negative.");
            }

            if (!AssetBundlePathIsAvailable(assetBundlePath))
            {
                throw new ArgumentException(Core.Utility.Text.Format("Already has an asset bundle with path '{0}'",
                    assetBundlePath));
            }

            var segments = assetBundlePath.Split('/');

            var assetBundleInfo = new AssetBundleInfo
            {
                Name = segments[segments.Length - 1],
                Path = assetBundlePath,
                GroupId = assetBundleGroup,
                DontPack = DontPack,
                IsDirectory = false,
            };

            AddAssetBundleInfoIntoTree(assetBundleInfo);

            m_ConfigCache.AssetBundleInfos.Add(assetBundlePath, new AssetBundleOrganizerConfig.AssetBundleInfo
            {
                AssetBundlePath = assetBundlePath,
                AssetBundleGroup = assetBundleGroup,
                DontPack = DontPack,
            });
            return assetBundleInfo;
        }

        public bool DeleteAssetBundle(string assetBundlePath)
        {
            if (!AssetBundlePathIsValid(assetBundlePath))
            {
                return false;
            }

            var node = GetAssetBundleInfo(assetBundlePath);

            if (node == null || node.IsDirectory)
            {
                return false;
            }

            RemoveAssetBundleInfoFromTree(node);

            foreach (var toRemove in m_ConfigCache.AssetInfos.Values.Where(ai => ai.AssetBundlePath == assetBundlePath).ToList())
            {
                m_ConfigCache.AssetInfos.Remove(toRemove.Guid);
                m_IncludedAssetGuidToInfoMap[toRemove.Guid].AssetBundlePath = string.Empty;
            }

            m_ConfigCache.AssetBundleInfos.Remove(assetBundlePath);
            return true;
        }

        public bool AssetBundlePathIsAvailable(string assetBundlePath)
        {
            var segments = assetBundlePath.Split('/');
            var node = m_AssetBundleInfoTreeRoot;
            bool available = false;
            for (int i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                if (!node.Children.ContainsKey(segment))
                {
                    available = true;
                    break;
                }

                node = node.Children[segment];
            }

            return available;
        }

        public static bool AssetBundlePathIsValid(string assetBundlePath)
        {
            if (assetBundlePath == string.Empty)
            {
                return false;
            }

            var segments = assetBundlePath.Split('/');

            for (int i = 0; i < segments.Length; i++)
            {
                if (!AssetBundlePathSegmentRegex.IsMatch(segments[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public void UnassignAssetFromBundle(AssetInfoInBundle assetInfoInBundle, string assetBundlePath)
        {
            var assetBundleInfo = GetAssetBundleInfo(assetBundlePath);
            if (assetBundleInfo == null)
            {
                throw new ArgumentException(Core.Utility.Text.Format("Asset bundle info not found at path '{0}'", assetBundlePath));
            }

            var rawAssetBundleInfo = m_ConfigCache.AssetBundleInfos[assetBundlePath];
            if (rawAssetBundleInfo == null)
            {
                throw new ArgumentException(Core.Utility.Text.Format("Asset bundle info not found at path '{0}'", assetBundlePath));
            }

            if (m_IncludedAssetGuidToInfoMap.TryGetValue(assetInfoInBundle.Guid, out var assetInfo) && assetInfo != null)
            {
                assetInfo.AssetBundlePath = null;
            }

            m_ConfigCache.AssetInfos.Remove(assetInfoInBundle.Guid);
            rawAssetBundleInfo.AssetGuids.RemoveAll(guid => guid == assetInfoInBundle.Guid);

            var rawAncestorBundleInfo = GetAncestorBundle(assetInfo);
            if (rawAncestorBundleInfo != null)
            {
                RemoveSameBundlePathInAssetInfoTree(assetInfo, rawAncestorBundleInfo);
            }
        }

        private AssetBundleOrganizerConfig.AssetBundleInfo GetAncestorBundle(AssetInfo assetInfo)
        {
            string ancestorBundlePath = null;
            for (var ai = assetInfo.Parent; ai != null; ai = ai.Parent)
            {
                if (string.IsNullOrEmpty(ai.AssetBundlePath))
                {
                    continue;
                }

                ancestorBundlePath = ai.AssetBundlePath;
                break;
            }

            if (ancestorBundlePath == null)
            {
                return null;
            }

            var rawAncestorBundleInfo = m_ConfigCache.AssetBundleInfos[ancestorBundlePath];
            if (rawAncestorBundleInfo == null)
            {
                throw new InvalidOperationException(
                    Core.Utility.Text.Format("Oops! Ancestor asset has been assigned to an AssetBundle, but now we cannot" +
                                             "find it whose path is '{0}'", ancestorBundlePath));
            }

            return rawAncestorBundleInfo;
        }

        public IList<AssetInfoInBundle> GetAssetInfosFromBundle(AssetBundleInfo assetBundleInfo)
        {
            if (assetBundleInfo == null)
            {
                throw new ArgumentNullException(nameof(assetBundleInfo));
            }

            if (assetBundleInfo.IsDirectory)
            {
                throw new ArgumentException("Asset bundle info is a directory.");
            }

            var rawAssetBundleInfo = m_ConfigCache.AssetBundleInfos[assetBundleInfo.Path];
            if (rawAssetBundleInfo == null)
            {
                throw new InvalidOperationException("Oops, cannot find the raw asset bundle info.");
            }

            return rawAssetBundleInfo.AssetGuids.ConvertAll(guid => new AssetInfoInBundle {Guid = guid});
        }

        public void AssignAssetsToBundle(IList<AssetInfo> assetInfos, string assetBundlePath)
        {
            if (assetInfos == null || assetInfos.Count <= 0)
            {
                throw new ArgumentException("Shouldn't be null or empty.", nameof(assetInfos));
            }

            foreach (var assetInfo in assetInfos)
            {
                if (assetInfo == null)
                {
                    throw new ArgumentException("Contains invalid or illegal AssetInfo.", nameof(assetInfos));
                }
            }

            foreach (var assetInfo in assetInfos)
            {
                UnassignAssetFromBundle(assetInfo);
            }

            var assetBundleInfo = GetAssetBundleInfo(assetBundlePath);

            if (assetBundleInfo == null)
            {
                throw new ArgumentNullException(nameof(assetBundlePath), "Shouldn't be null.");
            }

            if (assetBundleInfo.IsDirectory)
            {
                throw new ArgumentException("Shouldn't be a directory.", nameof(assetBundlePath));
            }

            var rawAssetBundleInfo = m_ConfigCache.AssetBundleInfos[assetBundleInfo.Path];
            if (rawAssetBundleInfo == null)
            {
                throw new ArgumentException(Core.Utility.Text.Format("Config doesn't contain asset bundle info with path '{0}'.",
                    assetBundleInfo.Path));
            }

            foreach (var assetInfo in assetInfos)
            {
                if (AssetDatabase.GetMainAssetTypeAtPath(assetInfo.Path) == null)
                {
                    continue;
                }

                bool shouldContinue = false;
                for (var node = assetInfo.Parent; node != null; node = node.Parent)
                {
                    if (string.IsNullOrEmpty(node.AssetBundlePath))
                    {
                        continue;
                    }

                    if (node.AssetBundlePath == assetBundlePath)
                    {
                        shouldContinue = true;
                    }

                    break;
                }

                if (shouldContinue)
                {
                    RemoveSameBundlePathInAssetInfoTree(assetInfo, rawAssetBundleInfo);
                    continue;
                }

                assetInfo.AssetBundlePath = assetBundlePath;
                rawAssetBundleInfo.AssetGuids.Add(assetInfo.Guid);
                if (!m_ConfigCache.AssetInfos.TryGetValue(assetInfo.Guid, out var rawAssetInfo))
                {
                    rawAssetInfo = new AssetBundleOrganizerConfig.AssetInfo {Guid = assetInfo.Guid, AssetBundlePath = assetBundleInfo.Path};
                    m_ConfigCache.AssetInfos.Add(rawAssetInfo.Guid, rawAssetInfo);
                }
                else
                {
                    rawAssetInfo.AssetBundlePath = assetBundleInfo.Path;
                }

                assetInfo.AssetBundlePath = assetBundleInfo.Path;
                RemoveSameBundlePathInAssetInfoTree(assetInfo, rawAssetBundleInfo);
            }
        }

        public int CleanUpInvalidAssets()
        {
            int assetRemoveCount = 0;
            var toRemove = m_ConfigCache.AssetInfos.Values.Where(
                ai => (new BaseAssetInfo {Guid = ai.Guid}.IsNullOrMissing) || !m_IncludedAssetGuidToInfoMap.ContainsKey(ai.Guid)).ToList();
            foreach (var assetInfo in toRemove)
            {
                m_ConfigCache.AssetInfos.Remove(assetInfo.Guid);
                assetRemoveCount++;
            }

            int assetInBundleRemoveCount = 0;
            foreach (var abi in m_ConfigCache.AssetBundleInfos.Values)
            {
                assetInBundleRemoveCount += abi.AssetGuids.RemoveAll(
                    guid => (new BaseAssetInfo {Guid = guid}.IsNullOrMissing || !m_IncludedAssetGuidToInfoMap.ContainsKey(guid)));
            }

            if (assetInBundleRemoveCount != assetRemoveCount)
            {
                throw new InvalidOperationException(Core.Utility.Text.Format(
                    "Inconsistent data with assetInBundleRemoveCount={0} and assetRemoveCount={1}.",
                    assetInBundleRemoveCount, assetRemoveCount));
            }

            return assetRemoveCount;
        }

        public AssetBundleInfo GetAssetBundleInfo(string assetBundlePath)
        {
            var segments = assetBundlePath.Split('/');
            var node = m_AssetBundleInfoTreeRoot;
            foreach (var segment in segments)
            {
                node = node.Children[segment];
                if (node == null)
                {
                    return null;
                }
            }

            return node;
        }

        public AssetInfo GetAssetInfo(string assetGuid)
        {
            m_IncludedAssetGuidToInfoMap.TryGetValue(assetGuid, out var ret);
            return ret;
        }

        private void LogAssetInfoForest()
        {
            foreach (var root in m_AssetInfoForestRoots)
            {
                LogAssetInfoTree(root);
            }
        }

        private void LogAssetInfoTree(AssetInfo root)
        {
            Debug.Log(root.ToString());
            foreach (var child in root.Children.Values)
            {
                LogAssetInfoTree(child);
            }
        }

        private void LogAssetBundleInfoTree(AssetBundleInfo root)
        {
            Debug.Log(root.ToString());
            foreach (var child in root.Children.Values)
            {
                LogAssetBundleInfoTree(child);
            }
        }

        private void RefreshAssetBundleNamesInAssetForest(string oldAssetBundlePath, string newAssetBundlePath)
        {
            var rawAssetInfos = m_ConfigCache.AssetInfos.Values.Where(ai => ai.AssetBundlePath == oldAssetBundlePath);
            foreach (var rawAssetInfo in rawAssetInfos)
            {
                rawAssetInfo.AssetBundlePath = newAssetBundlePath;
                if (rawAssetInfo.Guid == null)
                {
                    continue;
                }

                var assetPath = AssetDatabase.GUIDToAssetPath(rawAssetInfo.Guid);
                if (string.IsNullOrEmpty(assetPath))
                {
                    continue;
                }

                var assetInfo = GetAssetInfo(rawAssetInfo.Guid);
                if (assetInfo == null)
                {
                    continue;
                }

                assetInfo.AssetBundlePath = newAssetBundlePath;
            }
        }

        private void AddAssetBundleInfoIntoTree(AssetBundleInfo assetBundleInfo)
        {
            var segments = assetBundleInfo.Path.Split('/');
            var node = m_AssetBundleInfoTreeRoot;
            var path = string.Empty;
            for (int i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                path += "/" + segment;

                if (i == segments.Length - 1)
                {
                    assetBundleInfo.Parent = node;
                    node.Children.Add(segment, assetBundleInfo);
                }
                else
                {
                    if (!node.Children.ContainsKey(segment))
                    {
                        node.Children.Add(segment, new AssetBundleInfo
                        {
                            Name = segment,
                            Path = path,
                            Parent = node,
                            GroupId = Core.Asset.Constant.CommonResourceGroupId,
                            IsDirectory = i < segments.Length - 1,
                        });
                    }
                }

                node = node.Children[segment];
            }
        }

        private void UnassignAssetFromBundle(AssetInfo assetInfo)
        {
            if (assetInfo == null)
            {
                throw new ArgumentNullException(nameof(assetInfo));
            }

            if (!m_ConfigCache.AssetInfos.TryGetValue(assetInfo.Guid, out var rawAssetInfo))
            {
                return;
            }

            var assetBundlePath = assetInfo.AssetBundlePath;
            assetInfo.AssetBundlePath = null;
            m_ConfigCache.AssetInfos.Remove(rawAssetInfo.Guid);
            var rawAssetBundleInfo = m_ConfigCache.AssetBundleInfos[assetBundlePath];
            rawAssetBundleInfo.AssetGuids.RemoveAll(guid => guid == assetInfo.Guid);

            var rawAncestorBundleInfo = GetAncestorBundle(assetInfo);
            if (rawAncestorBundleInfo != null)
            {
                RemoveSameBundlePathInAssetInfoTree(assetInfo, rawAncestorBundleInfo);
            }
        }

        private int CompareRootAssetDirectories(AssetBundleOrganizerConfig.RootDirectoryInfo a,
            AssetBundleOrganizerConfig.RootDirectoryInfo b)
        {
            if (a.DirectoryGuid == null && b.DirectoryGuid == null)
            {
                return 0;
            }

            if (a.DirectoryGuid == null)
            {
                return -1;
            }

            if (b.DirectoryGuid == null)
            {
                return 1;
            }

            return AssetDatabase.GUIDToAssetPath(a.DirectoryGuid).CompareTo(AssetDatabase.GUIDToAssetPath(b.DirectoryGuid));
        }

        private bool AssetIsInSubRootDir(AssetBundleOrganizerConfig.RootDirectoryInfo rootDir, string rootAssetPath, string assetPath)
        {
            return m_ConfigCache.RootDirectoryInfos.Any(dirInfo
                => dirInfo != rootDir
                   && AssetDatabase.GUIDToAssetPath(dirInfo.DirectoryGuid).StartsWith(rootAssetPath)
                   && assetPath.StartsWith(AssetDatabase.GUIDToAssetPath(dirInfo.DirectoryGuid)));
        }

        private void BuildAssetTree(AssetBundleOrganizerConfig.RootDirectoryInfo rootDir)
        {
            if (rootDir.DirectoryGuid == null)
            {
                return;
            }

            var rootDirAssetPath = AssetDatabase.GUIDToAssetPath(rootDir.DirectoryGuid);
            if (string.IsNullOrEmpty(rootDirAssetPath) || Utility.Path.IsEditor(rootDirAssetPath))
            {
                return;
            }

            var rootDirAssetObj = AssetDatabase.LoadAssetAtPath<Object>(rootDirAssetPath);
            if (rootDirAssetObj == null)
            {
                return;
            }

            var root = new AssetInfo
            {
                Guid = rootDir.DirectoryGuid,
                Name = rootDirAssetObj.name,
            };

            root.AssetBundlePath = GetAssetBundlePath(root.Path);
            if (root.IsNullOrMissing)
            {
                return;
            }

            m_AssetInfoForestRoots.Add(root);
            m_IncludedAssetGuidToInfoMap.Add(root.Guid, root);

            var assetGuids = AssetDatabase.FindAssets(rootDir.Filter, new[] {root.Path});
            foreach (var assetGuid in assetGuids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                if (AssetIsInSubRootDir(rootDir, root.Path, assetPath))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(assetPath) || Utility.Path.IsEditor(assetPath))
                {
                    continue;
                }

                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    continue;
                }

                var mainAssetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                if (mainAssetType == typeof(MonoScript))
                {
                    continue;
                }

                var assetRelPathToRoot = Regex.Replace(assetPath, @"^" + root.Path, string.Empty);
                assetRelPathToRoot = Regex.Replace(assetRelPathToRoot, @"^/", string.Empty);
                var assetRelPathSegments = assetRelPathToRoot.Split('/');
                var node = root;
                var currentRelPath = string.Empty;
                for (int i = 0; i < assetRelPathSegments.Length; i++)
                {
                    var segment = assetRelPathSegments[i];
                    currentRelPath += currentRelPath == string.Empty ? segment : ("/" + segment);

                    if (node.Children.ContainsKey(segment))
                    {
                        node = node.Children[segment];
                    }
                    else
                    {
                        var newAssetPath = root.Path + "/" + currentRelPath;
                        var guid = AssetDatabase.AssetPathToGUID(newAssetPath);
                        var newNode = new AssetInfo
                        {
                            Name = segment,
                            Guid = guid,
                            Parent = node,
                            AssetBundlePath = GetAssetBundlePath(newAssetPath),
                        };

                        node.Children.Add(segment, newNode);
                        node = newNode;
                        m_IncludedAssetGuidToInfoMap.Add(node.Guid, node);
                    }
                }
            }
        }

        private string GetAssetBundlePath(string assetPath)
        {
            m_ConfigCache.AssetInfos.TryGetValue(AssetDatabase.AssetPathToGUID(assetPath), out var rawAssetInfo);
            return rawAssetInfo == null ? string.Empty : rawAssetInfo.AssetBundlePath;
        }

        private void RemoveSameBundlePathInAssetInfoTree(AssetInfo assetInfo, AssetBundleOrganizerConfig.AssetBundleInfo rawAssetBundleInfo)
        {
            foreach (var child in assetInfo.Children.Values)
            {
                if (!string.IsNullOrEmpty(child.AssetBundlePath) && child.AssetBundlePath != rawAssetBundleInfo.AssetBundlePath)
                {
                    continue;
                }

                foreach (var assetInfo2 in m_ConfigCache.AssetInfos.Values.Where(ai => AssetDatabase.GUIDToAssetPath(ai.Guid) == child.Path)
                    .ToList())
                {
                    m_ConfigCache.AssetInfos.Remove(assetInfo2.Guid);
                }

                rawAssetBundleInfo.AssetGuids.RemoveAll(guid => AssetDatabase.GUIDToAssetPath(guid) == child.Path);
                RemoveSameBundlePathInAssetInfoTree(child, rawAssetBundleInfo);
                child.AssetBundlePath = string.Empty;
            }
        }

        private void RemoveAssetBundleInfoFromTree(AssetBundleInfo assetBundleInfo)
        {
            var segments = assetBundleInfo.Path.Split('/');
            for (int i = segments.Length - 1; i >= 0; i--)
            {
                assetBundleInfo = assetBundleInfo.Parent;
                assetBundleInfo.Children[segments[i]].Parent = null;
                assetBundleInfo.Children.Remove(segments[i]);
                if (assetBundleInfo.Children.Count > 0)
                {
                    break;
                }
            }
        }
    }
}