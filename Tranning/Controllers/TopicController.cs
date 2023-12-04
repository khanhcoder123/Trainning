using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tranning.DataDBContext;
using Tranning.Models;

namespace Tranning.Controllers
{
    public class TopicController : Controller
    {
        private readonly TranningDBContext _dbContext;
        private readonly ILogger<TopicController> _logger;

        public TopicController(TranningDBContext context, ILogger<TopicController> logger)
        {
            _dbContext = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            TopicModel topicModel = new TopicModel();
            topicModel.TopicDetailLists = _dbContext.Topics
                .Select(item => new TopicDetail
                {
                    course_id = item.course_id,
                    id = item.id,
                    name = item.name,
                    description = item.description,
                    videos = item.videos,
                    status = item.status,
                    attach_file = item.attach_file,
                    documents  = item.documents,
                    created_at = item.created_at,
                    updated_at = item.updated_at
                }).ToList();

            return View(topicModel);
        }

        [HttpGet]
        public IActionResult Add()
        {
            TopicDetail topic = new TopicDetail();
            PopulateCategoryDropdown();
            return View(topic);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(TopicDetail topic, IFormFile Photo)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        string uniqueFileName = await UploadFile(Photo);
                        var topicData = new Topic()
                        {
                            course_id= topic.course_id,
                            name = topic.name,
                            description = topic.description,
                            videos = uniqueFileName,
                            status = topic.status,
                            documents = topic.documents,
                            attach_file = topic.attach_file,
                            created_at = DateTime.Now
                        };

                        _dbContext.Topics.Add(topicData);
                        _dbContext.SaveChanges();
                        TempData["saveStatus"] = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while processing a valid model state.");
                        TempData["saveStatus"] = false;
                    }
                    return RedirectToAction(nameof(Index));
                }

                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogError($"ModelState Error: {error.ErrorMessage}");
                    }
                }

                PopulateCategoryDropdown();
                return View(topic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while processing the request.");
                TempData["saveStatus"] = false;
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<string> UploadFile(IFormFile file)
        {
            string uniqueFileName;
            try
            {
                string pathUploadServer = "wwwroot\\uploads\\images";
                string fileName = file.FileName;
                fileName = Path.GetFileName(fileName);
                string uniqueStr = Guid.NewGuid().ToString();
                fileName = uniqueStr + "-" + fileName;
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), pathUploadServer, fileName);
                var stream = new FileStream(uploadPath, FileMode.Create);
                await file.CopyToAsync(stream);
                uniqueFileName = fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during file upload.");
                uniqueFileName = ex.Message.ToString();
            }
            return uniqueFileName;
        }

        private void PopulateCategoryDropdown()
        {
            try
            {
                var courses = _dbContext.Courses
                    .Where(m => m.deleted_at == null)
                    .Select(m => new SelectListItem { Value = m.id.ToString(), Text = m.name })
                    .ToList();

                if (courses != null)
                {
                    ViewBag.Stores = courses;
                }
                else
                {
                    _logger.LogError("Course is null");
                    ViewBag.Stores = new List<SelectListItem>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while populating category dropdown.");
                ViewBag.Stores = new List<SelectListItem>();
            }
        }      
    }
}
