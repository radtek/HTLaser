﻿using MES;
using Opc.Ua;
using OpcUaHelper;
using System;
using System.Collections;

namespace ProductManage.Core
{
    public class CollectHelper
    {
        public static ArrayList CollectWeldData(OpcUaClient OpcUaClient, string[] Nodes)
        {
            ArrayList objs = new ArrayList();

            int index = 0;
            try
            {
                foreach (DataValue node in OpcUaClient.ReadNodes(Nodes))
                {
                    // 获取到了值，具体的每个变量的解析参照上面类型不确定的解析
                    object value = node.WrappedValue.Value;

                    if (value != null)
                    {
                        objs.Add(value);
                    }
                    else
                    {
                        Program.LogNet.WriteError("采集OPC节点异常", "节点名：" + Nodes[index] + ",读取失败！");
                        break;
                    }
                    index++;
                }
            }
            catch (Exception ex)
            {
                objs = null;
                Program.LogNet.WriteError("异常", "批量采集OPC数据异常：" + ex.Message);
            }

            return objs;
        }

    }
}
