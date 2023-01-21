using System;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class IPUtils
{
    public static string[] GetIPList()
    {
        var listOfAddresses = GetDnsAddresses();
        var addressString = new List<string>();
        foreach (var address in listOfAddresses)
        {
            addressString.Add(address.ToString());
        }

        return addressString.ToArray();
    }
    public static string GetBroadcastIP()
    {
        var   index = 3;
#if UNITY_ANDROID
         index = 1;
#endif
        var addresses = GetDnsAddresses();
        foreach (var ipAddress in addresses)
        {
            Debug.Log(ipAddress.ToString());
        }

        var defaultIP = "127.0.0.1";
        try
        {
            defaultIP = addresses[index].ToString();
        }
        catch (Exception e)
        {
            // ignored
        }

        return defaultIP;
    }

    public static IPAddress[] GetDnsAddresses()
    {
        return System.Net.Dns.GetHostAddresses("");
    }

    public static string GetHttpPort() => "4444";
    public static string GetSocketPort() => "7777";
    public static int GetSocketPortInt() => 7777;
}