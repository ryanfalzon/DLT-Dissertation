﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnifiedModel.SourceGenerator.CommonModels;

namespace UnifiedModel.SourceGenerator.SourceGenerators
{
    public class XChainGeneratorFactory
    {
        public Dictionary<XChains, IXChainGenerator> XChainGenerators { get; set; }

        public XChainGeneratorFactory()
        {
            XChainGenerators = new Dictionary<XChains, IXChainGenerator>();
        }

        public List<IXChainGenerator> Get(string xChain, string xOnChain = null)
        {
            if (!string.IsNullOrEmpty(xChain))
            {
                var xChainGenerators = new List<IXChainGenerator>();

                switch (xChain)
                {
                    case Constants.XAll:
                        {
                            foreach (XChains currentXChain in (XChains[])Enum.GetValues(typeof(XChains)))
                            {
                                switch (currentXChain)
                                {
                                    case XChains.OffChain:
                                        {
                                            xChainGenerators.AddRange(Get(Constants.XOffChain));
                                            break;
                                        }

                                    case XChains.EthereumChain:
                                        {
                                            xChainGenerators.AddRange(Get(Constants.XOnChain, Constants.XOnEthereumChain));
                                            break;
                                        }
                                }
                            }
                            break;
                        }

                    case Constants.XOffChain:
                        {
                            EnsureExists<XOffChainGenerator>(XChains.OffChain);
                            xChainGenerators.Add(XChainGenerators[XChains.OffChain]);
                            break;
                        }

                    case Constants.XOnChain:
                        {
                            if (!string.IsNullOrEmpty(xOnChain))
                            {
                                xOnChain = xOnChain.Contains("\"") ? xOnChain.Replace("\"", "") : xOnChain;

                                switch (xOnChain)
                                {
                                    case "Ethereum":
                                        {
                                            EnsureExists<XOnChainEthereumGenerator>(XChains.EthereumChain);
                                            xChainGenerators.Add(XChainGenerators[XChains.EthereumChain]);
                                            break;
                                        }

                                    default: throw new InvalidEnumArgumentException("Invalid XOnChain token!");
                                }
                            }
                            else
                            {
                                throw new ArgumentNullException("No XChain provided!");
                            }

                            break;
                        }

                    default: throw new InvalidEnumArgumentException("Invalid XChain token!");
                }

                return xChainGenerators;
            }
            else
            {
                throw new ArgumentNullException("No XChain provided!");
            }
        }

        public void EnsureExists<T>(XChains xChain)
        {
            if (!XChainGenerators.ContainsKey(xChain))
            {
                XChainGenerators.Add(xChain, (IXChainGenerator)Activator.CreateInstance(typeof(T)));
            }
        }

        public List<(string filename, string contents)> Consume()
        {
            var files = new List<(string filename, string contents)>();

            foreach (var xChainGenerator in XChainGenerators)
            {
                xChainGenerator.Value.Consume();
                files.Add((xChainGenerator.Key.ToString(), xChainGenerator.Value.ToString()));
            }

            return files;
        }
    }
}