using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;

namespace EmployeeManagement.Controllers
{
    public class HomeController : Controller
    {
        //here we are injecting IEmployeeRepository to the HomeContoller.
        private readonly IEmployeeRepository _employeeRepository; // injected Dependancy

        public readonly IHostingEnvironment hostingEnvironment; 
        public HomeController(IEmployeeRepository employeeRepository,
                               IHostingEnvironment hostingEnvironment)
        {
            _employeeRepository = employeeRepository;
            this.hostingEnvironment = hostingEnvironment;
        }

        public ViewResult Index()
        {
            var model = _employeeRepository.GetAllEmployees();
            return View(model);
        }
  
        public ViewResult Details(int? id)
        {
            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                Employee = _employeeRepository.GetEmployee(id ?? 1),
                PageTitle = "Employee Details"
            };
            return View(homeDetailsViewModel);
        }
        [HttpGet]
        public ViewResult Edit(int id)
        {
            Employee employee = _employeeRepository.GetEmployee(id);
            EmployeeEditViewModel employeeEditViewModel = new EmployeeEditViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                Email =employee.Email,
                Department =employee.Department,
                ExistingPhotoPath = employee.PhotoPath
            };
            return View(employeeEditViewModel);
        }

        /*Through model binding, the action method parameter EmployeeEditViewModel
          receives the posted edit form data*/

        [HttpPost]
        public IActionResult Edit(EmployeeEditViewModel model)
        {
            /* Check if the provided data is valid, if not renderd the edit view
             * so the user can correct and resubmit the edit form*/
            if (ModelState.IsValid)
            {

                //Returve the employee being edited form the database

                Employee employee = _employeeRepository.GetEmployee(model.Id);
                // Update the employee object with the data in the model object

                employee.Name = model.Name;
                employee.Email = model.Email;
                employee.Department = model.Department;

                /* if the user want to change the photo, a new photowill be uploded and
                 * the Photo property on the model object receives the uploaded photo. 
                 * if the Photo property is null, user did not upload a new photo and
                 * keep his existing photo.*/

                if (model.Photo != null)
                {
                    //if a new Photo is uploaded, the existing Photo must be deleted.
                    //So check if there is an existing photo and delete.
                    if (model.ExistingPhotoPath != null)
                    {
                        string filePath = Path.Combine(hostingEnvironment.WebRootPath,
                            "images", model.ExistingPhotoPath);
                        System.IO.File.Delete(filePath);
                    }
                    /* Save the new photo in wwwroot/images folder and update PhotoPath proprty 
                     * of the employee object which will be eventually saved in the database*/
                    employee.PhotoPath = ProcessUploadedFile(model);
                }
                 /* Call update method on the reporsitory services passing it the employee object
                  * to update the data in the database table*/
                    Employee updateEmployee = _employeeRepository.Update(employee);
                return RedirectToAction("index");
            }
            return View();

        }

        //This method same code is used by 2 other method
        //1.IActionResult Create
        //2.IActionResult Edit

        private string ProcessUploadedFile(EmployeeCreateViewModel model)
        {
            string uniqueFilename = null;

            // If the Photo property on the incoming model object is not null, then the user
            // has selected an image to upload.
            if (model.Photo != null)
            {
                // Loop thru each selected file to uplaod multiple files
               // foreach (IFormFile photo in model.Photos)
                //{ ForEach Loop begin

                    /* The image must be uploaded to the images folder in wwwroot
                     To get the path of the wwwroot folder we are using the inject
                     HostingEnvironment service provided by ASP.NET Core*/
                    string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");

                    /* To make sure the file name is unique we are appending a new
                     GUID value and an underscore to the file name*/
                    uniqueFilename = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFilename);

                /*Use CopyTo() method provided by IFormFile interface to
                copy the file to wwwroot/Images folder*/
                using (var fileSteam = new FileStream(filePath, FileMode.Create))
                {
                    model.Photo.CopyTo(fileSteam);
                }
                //} ForEach loop End 
            }

            return uniqueFilename;
        }

        [HttpGet]
        public ViewResult Create() {
            return View();
        }
        [HttpPost]
        public IActionResult Create(EmployeeCreateViewModel model) {

            if (ModelState.IsValid) {
                string uniqueFilename = ProcessUploadedFile(model);

                Employee newEmployee = new Employee
                {
                    Name = model.Name,
                    Email = model.Email,
                    Department = model.Department,
                    //Store the file name  in Photopath property of the employee object 
                    //which gets saved to the Employees database table.
                    PhotoPath =uniqueFilename
                };
                _employeeRepository.Add(newEmployee);
                return RedirectToAction("details", new { id = newEmployee.Id });
            }
            return View();
            
        }
    }
}
