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
    public class CourseController : Controller
    {
        private readonly TranningDBContext _dbContext;
        private readonly ILogger<CourseController> _logger;

        public CourseController(TranningDBContext context, ILogger<CourseController> logger)
        {
            _dbContext = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index(string SearchString)
        {
            CourseModel courseModel = new CourseModel();

            // Retrieve all courses from the database
            var data = _dbContext.Courses
                .Where(m => m.deleted_at == null);

            // Filter courses based on the search string
            if (!string.IsNullOrEmpty(SearchString))
            {
                data = data.Where(m => m.name.Contains(SearchString) || m.description.Contains(SearchString));
            }

            // Project courses to CourseDetail and convert to a list
            courseModel.CourseDetailLists = data
                .Select(item => new CourseDetail
                {
                    category_id = item.category_id,
                    id = item.id,
                    name = item.name,
                    description = item.description,
                    avatar = item.avatar,
                    status = item.status,
                    start_date = item.start_date,
                    end_date = item.end_date,
                    created_at = item.created_at,
                    updated_at = item.updated_at
                }).ToList();

            return View(courseModel);
        }


        [HttpGet]
        public IActionResult Add()
        {
            CourseDetail course = new CourseDetail();
            PopulateCategoryDropdown();
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(CourseDetail course, IFormFile Photo)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        string uniqueFileName = await UploadFile(Photo);
                        var courseData = new Course()
                        {
                            category_id = course.category_id,
                            name = course.name,
                            description = course.description,
                            avatar = uniqueFileName,
                            status = course.status,
                            start_date = course.start_date,
                            end_date = course.end_date,
                            created_at = DateTime.Now
                        };

                        _dbContext.Courses.Add(courseData);
                        _dbContext.SaveChanges(true);
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
                return View(course);
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
                var categories = _dbContext.Categories
                    .Where(m => m.deleted_at == null)
                    .Select(m => new SelectListItem { Value = m.id.ToString(), Text = m.name })
                    .ToList();

                if (categories != null)
                {
                    ViewBag.Stores = categories;
                }
                else
                {
                    _logger.LogError("Categories is null");
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
            CourseDetail course = new CourseDetail();
            var data = _dbContext.Courses.Where(m => m.id == id).FirstOrDefault();
            if (data != null)
            {
                course.id = data.id;
                course.name = data.name;
                course.description = data.description;
                course.avatar = data.avatar;
                course.start_date = data.start_date;
                course.end_date = data.end_date;
                course.status = data.status;
                course.category_id = data.category_id;

                ViewBag.Stores ??= _dbContext.Categories
                                    .Select(c => new SelectListItem { Value = c.id.ToString(), Text = c.name })
                                    .ToList();
            }

            return View(course);
        }

        [HttpPost]
        public async Task<IActionResult> Update(CourseDetail course, IFormFile file)
        {
            try
            {
                var data = _dbContext.Courses.Where(m => m.id == course.id).FirstOrDefault();
                string uniqueIconAvatar = "";
                if (course.Photo != null)
                {
                    uniqueIconAvatar = await UploadFile(course.Photo);
                }

                if (data != null)
                {
                    data.name = course.name;
                    data.description = course.description;
                    data.start_date = course.start_date;
                    data.end_date = course.end_date;
                    data.status = course.status;
                    data.category_id = course.category_id;

                    if (!string.IsNullOrEmpty(uniqueIconAvatar))
                    {
                        data.avatar = uniqueIconAvatar;
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
                var data = _dbContext.Courses.Where(m => m.id == id).FirstOrDefault();
                if (data != null)
                {
                    // Use Remove method to delete the entity
                    _dbContext.Courses.Remove(data);
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
