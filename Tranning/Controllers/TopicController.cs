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
        public IActionResult Index(string SearchString)
        {
            TopicModel topicModel = new TopicModel();

            // Retrieve all topics from the database
            var data = _dbContext.Topics
                .Where(m => m.deleted_at == null);

            // Filter topics based on the search string
            if (!string.IsNullOrEmpty(SearchString))
            {
                data = data.Where(m => m.name.Contains(SearchString) || m.description.Contains(SearchString));
            }

            // Project topics to TopicDetail and convert to a list
            topicModel.TopicDetailLists = data
                .Select(item => new TopicDetail
                {
                    course_id = item.course_id,
                    id = item.id,
                    name = item.name,
                    description = item.description,
                    videos = item.videos,
                    status = item.status,
                    attach_file = item.attach_file,
                    documents = item.documents,
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
        public async Task<IActionResult> Add(TopicDetail topic)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        string uniqueFileName = await UploadFile(topic.photo);
                        string file = await UploadFile(topic.file);
                        var topicData = new Topic()
                        {
                            course_id= topic.course_id,
                            name = topic.name,
                            description = topic.description,
                            videos = uniqueFileName,
                            status = topic.status,
                            documents = topic.documents,
                            attach_file = file,
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
        [HttpGet]
        public IActionResult Update(int id = 0)
        {
            TopicDetail topic = new TopicDetail();
            var data = _dbContext.Topics.Where(m => m.id == id).FirstOrDefault();
            if (data != null)
            {
                topic.id = data.id;
                topic.name = data.name;
                topic.course_id = data.course_id;
                topic.description = data.description;
                topic.status = data.status;
                topic.documents = data.documents;
            }

            PopulateCategoryDropdown(); // Make sure to populate the dropdown
            return View(topic);
        }

        [HttpPost]
        public async Task<IActionResult> Update(TopicDetail topic, IFormFile file)
        {
            try
            {
                var data = _dbContext.Topics.Where(m => m.id == topic.id).FirstOrDefault();
                string uniqueFileName = "";

                if (file != null)
                {
                    uniqueFileName = await UploadFile(file);
                }

                if (data != null)
                {
                    data.name = topic.name;
                    data.course_id = topic.course_id;
                    data.description = topic.description;
                    data.status = topic.status;
                    data.documents = topic.documents;

                    if (!string.IsNullOrEmpty(uniqueFileName))
                    {
                        data.attach_file = uniqueFileName;
                    }

                    await _dbContext.SaveChangesAsync();
                    TempData["UpdateStatus"] = true;
                }
                else
                {
                    TempData["UpdateStatus"] = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during the update operation.");
                TempData["UpdateStatus"] = false;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(int id = 0)
        {
            try
            {
                var data = _dbContext.Topics
                    .Where(m => m.id == id && m.deleted_at == null)
                    .FirstOrDefault();

                if (data != null)
                {
                    data.deleted_at = DateTime.Now; // Updated to the current date and time
                    _dbContext.SaveChanges();
                    TempData["DeleteStatus"] = true;
                }
                else
                {
                    TempData["DeleteStatus"] = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during the delete operation.");
                TempData["DeleteStatus"] = false;
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
