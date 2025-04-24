﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BuggyDemoWeb.Models;
using BuggyDemoCode.Services;
using System.Runtime.CompilerServices;
using BuggyDemoWeb.Services;

namespace BuggyDemoWeb.Controllers
{
    public class ErrorsExceptionsCrashController : Controller
    {
        private readonly LegacyService legacyService;
        private readonly AudioSpatialSignalProcess _audioService;
        private const int DATA_ID = 2;

        public ErrorsExceptionsCrashController(LegacyService legacyService, AudioSpatialSignalProcess audioService)
        {
            this.legacyService = legacyService;
            this._audioService = audioService;
        }

        public IActionResult Index()
        {
            return Ok();
        }

        /// <summary>
        /// Port exhaustion, apparently, not sure how to meausure it...
        /// </summary>
        /// <returns></returns>
        [HttpGet("exception/port-exhaustion")]
        public async Task<int> HttpClientPortExhaustion()
        {
            using (var httpClient = new HttpClient())
            {
                var result = await httpClient.GetAsync("https://github.com");
                return (int)result.StatusCode;
            }
        }

        /// <summary>
        /// Port exhaustion, apparently, not sure how to meausure it...
        /// </summary>
        /// <returns></returns>
        [HttpGet("exception/object-dispose")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public IActionResult ObjectDisposeException()
        {
            legacyService.CreateStreamReadByte();

            return Ok();
        }

        [HttpGet("exception/out-of-range")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task<IActionResult> OutOfRange()
        {
            var sb = await legacyService.ValidateThisCollection();

            return Ok(sb);
        }

        [HttpGet("exception/null-reference-exception")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public IActionResult NullReferenceException()
        {
            var sb = legacyService.ValidateAnotherCollection();

            return Ok(sb);
        }

        [HttpGet("exception/in-async-call-stack")]
        public async Task<IActionResult> OutOfRange2()
        {
            await legacyService.Alpha();

            return Ok();
        }

        [HttpGet("crash/stack-overflow")]
        public IActionResult StackOverflow()
        {
            legacyService.TypicalRecurrsionExample();

            return Ok();
        }

        [HttpGet("crash/stack-overflow2")]
        public IActionResult StackOverflow2()
        {
            legacyService.ATypicalRecurrsionExample();

            return Ok();
        }

        [HttpGet("crash/async-void1")]
        public IActionResult AsyncVoidCrash()
        {
            RaiseEvent();

            return Ok();
        }

        private void RaiseEvent() => RaiseEventVoidAsync();
        private async void RaiseEventVoidAsync() => throw new Exception("Error!");

        [HttpGet("crash/async-void2")]
        public async void AsyncVoidCrash2()
        {
            await Task.Delay(1000);

            // THIS will crash the process since we're writing after the response has completed on a background thread
            await Response.WriteAsync("Hello World");
        }

        /// <summary>
        /// Crashes after you create some load
        /// e.g. wrk -c 256 -t 10 -d 20 https://localhost:5001/crash/parallel-list-async
        /// </summary>
        /// <returns></returns>
        [HttpGet("crash/parallel-list-async")]
        public Task ParallelAsyncCrash()
        {
            var list = new List<int>();
            var tasks = new Task[10];

            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = GetPageDataAsync(list, i);
            }

            return Task.WhenAll(tasks);
        }

        [HttpGet("crash/async-void-delay")]
        public async void AsyncVoidDelay(string query)
        {
            var response = await legacyService.RetrieveData(DATA_ID);

            await Response.WriteAsync(response);
        }

        [HttpGet("crash/nan-call")]
        public async Task<QuadraticRoots> NanCall()
        {
            return await AudioCompressionRatio();
        }

        private async Task GetPageDataAsync(List<int> results, int number)
        {
            await Task.Delay(300); // Exchange with an IO bound call that will take some indeterminate time 100-300ms

            results.Add(number);
        }

        public async void RetrieveSupportInfo()
        {
            var response = await legacyService.RetrieveData(DATA_ID);

            await Response.WriteAsync(response);
        }

        public async Task<QuadraticRoots> AudioCompressionRatio()
        {
            var response = await _audioService.RetrieveQuadraticRoots(3, 4, 5);

            return response;
        }
    }
}
