using Microsoft.EntityFrameworkCore;

namespace FlowX.EntityFrameworkCore.Delegates;

internal delegate DbContext GetDbContext(Type modelType);
internal delegate DbContext[] GetDbContexts();