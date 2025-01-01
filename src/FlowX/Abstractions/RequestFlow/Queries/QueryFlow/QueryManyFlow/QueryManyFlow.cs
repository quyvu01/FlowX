﻿using System.Linq.Expressions;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

public class QueryManyFlow<TModel, TResponse> : IQueryListFilter<TModel, TResponse>,
    IQueryListSpecialAction<TModel, TResponse>,
    IQueryListMapResponse<TModel, TResponse>,
    IQueryListSortedField<TModel, TResponse>,
    IQueryListSortedDirection<TModel, TResponse>,
    IQueryListFlowBuilder<TModel, TResponse> where TModel : class
{
    public QuerySpecialActionType QuerySpecialActionType { get; private set; }
    public Expression<Func<TModel, bool>> Filter { get; private set; }
    public Func<IQueryable<TModel>, IQueryable<TModel>> SpecialActionToModel { get; private set; }
    public Func<IQueryable<TModel>, IQueryable<TResponse>> SpecialActionToResponse { get; private set; }
    public Func<TModel, TResponse> MapFunc { get; private set; }
    public Expression<Func<TModel, object>> SortFieldNameWhenRequestEmpty { get; private set; }
    public SortedDirection SortedDirectionWhenRequestEmpty { get; private set; }

    public IQueryListSpecialAction<TModel, TResponse> WithFilter(Expression<Func<TModel, bool>> filter)
    {
        Filter = filter;
        return this;
    }

    IQueryListMapResponse<TModel, TResponse> IQueryListSpecialAction<TModel, TResponse>.WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction)
    {
        QuerySpecialActionType = QuerySpecialActionType.ToModel;
        SpecialActionToModel = specialAction;
        return this;
    }

    public IQueryListSortedField<TModel, TResponse> WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TResponse>> specialAction)
    {
        QuerySpecialActionType = QuerySpecialActionType.ToTarget;
        SpecialActionToResponse = specialAction;
        return this;
    }

    public IQueryListSortedDirection<TModel, TResponse> WithSortFieldWhenNotSet(
        Expression<Func<TModel, object>> expression)
    {
        SortFieldNameWhenRequestEmpty = expression;
        return this;
    }

    public IQueryListFlowBuilder<TModel, TResponse> WithSortedDirectionWhenNotSet(SortedDirection direction)
    {
        SortedDirectionWhenRequestEmpty = direction;
        return this;
    }

    public IQueryListSortedField<TModel, TResponse> WithMap(Func<TModel, TResponse> mapFunc)
    {
        MapFunc = mapFunc;
        return this;
    }
}