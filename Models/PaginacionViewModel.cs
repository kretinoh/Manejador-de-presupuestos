namespace ManejoPresupuesto.Models
{
    public class PaginacionViewModel
    {
        public int Pagina { get; set; } = 1;
        private int recordsPorPagina = 10;
        private readonly int _cantidadMaximaRecordsPorPagina = 50;


        public int RecordPorpagina
        {
            get
            {
                return recordsPorPagina;
            }
            set
            {
                recordsPorPagina = (value > _cantidadMaximaRecordsPorPagina) 
                    ? _cantidadMaximaRecordsPorPagina 
                    : value;
            }
        }
        public int RecordsASaltar => recordsPorPagina*(Pagina - 1 );
    }
}
