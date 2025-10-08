//using Emp.Web.Dtos.Models;
using Emp.Api.Models;
using Emp.Web.Dtos.JobTitle;
using Emp.Web.Models.Dtos;
using EMP.Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EMP.Web.Controllers
{
    public class JobTitlesController : Controller
    {
        private readonly IJobTitleService _jobTitleService;
        public JobTitlesController(IJobTitleService jobTitleService)
        {
            _jobTitleService = jobTitleService;
        }
        public async Task<IActionResult> Index()
        {
            ResponseDto response = await _jobTitleService.GetJobTitlesAsync();
            var jobTitlesList = new List<Emp.Web.Dtos.JobTitle.JobTitleViewDto>();
            if (response.IsSuccess && response.Result is not null)
            {
                jobTitlesList = JsonConvert.DeserializeObject<List<JobTitleViewDto>>(response.Result.ToString());

                return View(jobTitlesList);
            }
            else
            {
                TempData["error"] = "Something went wrong!";
                return View();
            }
        }
        public async Task<IActionResult> Details(int id)
        {
            ResponseDto response = await _jobTitleService.GetJobTitleAsync(id);
            var jobTitleList = new JobTitleViewDto();
            if (response.IsSuccess && response.Result is not null)
            {
                jobTitleList = JsonConvert.DeserializeObject<Emp.Web.Dtos.JobTitle.JobTitleViewDto>(response.Result.ToString());
                return View(jobTitleList);
            }
            else
            {
                TempData["error"] = "Something went wrong!";
                return View();
            }
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(JobTitleCreateDto jobTitleCreate)
        {
            if (ModelState.IsValid)
            {
                var response = await _jobTitleService.CreateJobTitlesAsync(jobTitleCreate);
                if (response.IsSuccess)
                {
                    TempData["Success"] = response.Message;
                    return RedirectToAction("Index");
                }
                TempData["Error"] = "Something went wrong!";
                return View(jobTitleCreate);

            }
            else
            {
                return View(jobTitleCreate);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {

            ResponseDto response = await _jobTitleService.DeleteJobTitleAsync(id);
            if (response.IsSuccess && response.Result is not null)
            {
                TempData["Success"] = response.Message;
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = "Something went wrong!";
                return View();
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeletePost(int id)
        {
            //if (id>0)
            //{
            //    var response = await _countryService.DeleteCountryAsync(id);
            //    if (response.IsSuccess)
            //    {
            //        TempData["Success"] = response.Message;
            //        return RedirectToAction("Index");
            //    }
            //    else
            //    {
            //        TempData["Error"] = "Something went wrong";
            //        return RedirectToAction("Index");
            //    }
            //}
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ResponseDto response = await _jobTitleService.GetJobTitleAsync(id);
            var jobTitle= new JobTitleViewDto();
            if (response.IsSuccess && response.Result is not null)
            {
                jobTitle = JsonConvert.DeserializeObject<Emp.Web.Dtos.JobTitle.JobTitleViewDto>(response.Result.ToString());
                return View(jobTitle);
            }
            else
            {
                TempData["error"] = "Something went wrong!";
                return View();
            }
        }
        [HttpPost]
        public async Task<IActionResult> Edit(JobTitleCreateDto jobTitleCreateDto)
        {
            if (ModelState.IsValid)
            {
                ResponseDto response = await _jobTitleService.UpdateJobTitlesAsync(jobTitleCreateDto);
                if (response.IsSuccess && response.Result is not null)
                {
                    TempData["Success"] = response.Message;
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Something went wrong!";
                    return View();
                }
            }
            else
            {
                return View(jobTitleCreateDto);
            }
           
        }
    }
}
