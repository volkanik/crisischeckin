﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using crisicheckinweb.ViewModels;
using Models;
using Services.Interfaces;
using WebMatrix.WebData;

namespace crisicheckinweb.Controllers
{
    public class HomeController : BaseController
    {

        private readonly IDisaster _disasterSvc;
        private readonly IVolunteer _volunteerSvc;

        public HomeController(IDisaster disasterSvc, IVolunteer volunteerSvc)
        {
            _disasterSvc = disasterSvc;
            _volunteerSvc = volunteerSvc;
        }

        // GET: /Home/
        public ActionResult Index()
        {
            return View(GetDefaultViewModel());
        }

        [HttpPost]
        public ActionResult Assign(VolunteerViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (DateTime.Compare(model.SelectedEndDate, model.SelectedStartDate) >= 0)
                {
                    Person me = _volunteerSvc.FindByUserId(WebSecurity.CurrentUserId);
                    _disasterSvc.AssignToVolunteer(new Disaster { Id = model.SelectedDisaster },
                        me, model.SelectedStartDate, model.SelectedEndDate);
                }
                else
                {
                    ModelState.AddModelError("", "Start Date must come before End Date.");
                    var modelToReturn = GetDefaultViewModel();
                    modelToReturn.SelectedDisaster = model.SelectedDisaster;
                    modelToReturn.SelectedStartDate = model.SelectedStartDate;
                    modelToReturn.SelectedEndDate = model.SelectedEndDate;
                    return View("Index", modelToReturn);
                }

                return Redirect("/Home");
            }
            else
            {
                return View("Index", GetDefaultViewModel());
            }
        }

        private VolunteerViewModel GetDefaultViewModel()
        {
            var person = _volunteerSvc.FindByUserId(WebSecurity.CurrentUserId);
            IEnumerable<Commitment> comms = (person != null) ?
                _volunteerSvc.RetrieveCommitments(person, true) :
                new List<Commitment>().AsEnumerable();

            var model = new VolunteerViewModel
            {
                Disasters = _disasterSvc.GetActiveList(),
                MyCommitments = comms
            };

            return model;
        }

    }
}
