﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using ExternalServer.Common.Models.Entities;
using ExternalServer.Common.Specifications.DataObjects;

namespace ExternalServer.Common.Utilities {
    public static class CryptoUtils {

        /// <summary>
        /// https://docs.microsoft.com/en-us/dotnet/api/system.io.unmanagedmemorystream?view=net-5.0
        /// </summary>
        /// <param name="memIntPtr">Pointer to the unmanaged memory</param>
        /// <param name="length">Length of memory</param>
        public static void ObfuscateAndFreeMemory(IntPtr memIntPtr, long length) {
            if (memIntPtr == IntPtr.Zero)
                return;

            Random random = new Random((int)DateTime.Now.Ticks);

            unsafe {
                // Get a byte pointer from the IntPtr object.
                byte* memBytePtr = (byte*)memIntPtr.ToPointer();

                using (UnmanagedMemoryStream ums = new UnmanagedMemoryStream(memBytePtr, length, length, FileAccess.Write)) {
                    byte[] randomByteArray = new byte[length];
                    random.NextBytes(randomByteArray);

                    ums.Write(randomByteArray, 0, randomByteArray.Length);
                }
            }

            // free memory
            var gcHandle = GCHandle.FromIntPtr(memIntPtr);
            gcHandle.Free();
            if (memIntPtr != IntPtr.Zero) {
                Marshal.ZeroFreeBSTR(memIntPtr);
            }
        }

        /// <summary>
        /// Writes a byte array to unmanaged memory and obfuscates the original byte array.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Pointer to the unmanaged memory.</returns>
        public static IUnmanagedMemoryObject MoveDataToUnmanagedMemory(byte[] data) {
            IntPtr dataPtr = Marshal.AllocHGlobal(data.Length);
            unsafe {
                // Get a byte pointer from the IntPtr object and writes data using a UnmanagedMemoryStream
                byte* memBytePtr = (byte*)dataPtr.ToPointer();
                using (UnmanagedMemoryStream ums = new UnmanagedMemoryStream(memBytePtr, data.Length, data.Length, FileAccess.Write)) {
                    ums.Write(data, 0, data.Length);
                }
            }

            var umo = new UnmanagedMemoryObject {
                Pointer = dataPtr,
                Length = data.Length
            };
            ObfuscateByteArray(data);

            return umo;
        }

        /// <summary>
        /// Overwrites a byte array with random bytes.
        /// </summary>
        public static void ObfuscateByteArray(byte[] confidentialData) {
            Random random = new Random((int)DateTime.Now.Ticks);
            random.NextBytes(confidentialData);
        }

        /// <summary>
        /// Returns stored byte[] from intPtr.
        /// Does not delete the unmanaged memory.
        /// </summary>
        /// <param name="result">array to store the data in</param>
        /// <param name="intPtr">pointer pointing to the unmanaged memory</param>
        /// <param name="length">length of data</param>
        public static void GetByteArrayFromUM(byte[] result, IntPtr intPtr, int length) {
            if (result == null) {
                result = new byte[length];
            }

            Marshal.Copy(intPtr, result, 0, length);
        }
    }
}
