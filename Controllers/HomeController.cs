using CircuitSimulatorWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using CircuitSimulatorWeb.CircuitCode;
using Nancy.Json;
using CircuitSimulatorWeb.Models.CircuitStruc;

namespace CircuitSimulatorWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Help()
        {
            return View();
        }

        public IActionResult CircuitSim()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public JsonResult ReadJSON(string json)
        {
            Debug.WriteLine(json);
            try {             
                ComponentStruc[] structureIn = new JavaScriptSerializer().Deserialize<ComponentStruc[]>(json);
                Circuit circuit = new Circuit(structureIn);
                circuit.CalculateVoltages();
                circuit.updateComponents();
                double[] voltages = new double[circuit.components.Length];
                for (int i = 0; i < circuit.components.Length; i++)
                {
                    if (circuit.components[i] is EnergySource)
                    {
                        voltages[i] = Math.Abs(Math.Round(((EnergySource)circuit.components[i]).voltage, 2));
                    }
                    else if (circuit.components[i] is Voltmeter)
                    {
                        voltages[i] = Math.Abs(Math.Round(((ResistiveComponent)circuit.components[i]).GetVoltage(), 2));
                    }
                    else if (circuit.components[i] is Ammeter)
                    {
                        voltages[i] = Math.Abs(Math.Round(((ResistiveComponent)circuit.components[i]).current, 2));
                    }
                    else if (circuit.components[i] is ResistiveComponent)
                    {
                        voltages[i] = Math.Abs(Math.Round(((ResistiveComponent)circuit.components[i]).GetVoltage(), 2));
                    }
                }
                return new JsonResult(voltages);
            } catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }

        }
    }
}