﻿//
// This file is part of
// FsAlg: Generic Linear Algebra Library
//
// Copyright (c) 2015, National University of Ireland Maynooth (Atilim Gunes Baydin, Barak A. Pearlmutter)
//
// FsAlg is released under the BSD license.
// (See accompanying LICENSE file.)
//
// Written by:
//
//   Atilim Gunes Baydin
//   atilimgunes.baydin@nuim.ie
//
//   Barak A. Pearlmutter
//   barak@cs.nuim.ie
//
//   Brain and Computation Lab
//   Hamilton Institute & Department of Computer Science
//   National University of Ireland Maynooth
//   Maynooth, Co. Kildare
//   Ireland
//
//   www.bcl.hamilton.ie
//

namespace FsAlg.Generic

open FsAlg.Generic.Util

/// Generic matrix type
[<NoEquality; NoComparison>]
type Matrix<'T when 'T : (static member Zero : 'T)
                and 'T : (static member One : 'T)
                and 'T : (static member (+) : 'T * 'T -> 'T)
                and 'T : (static member (-) : 'T * 'T -> 'T)
                and 'T : (static member (*) : 'T * 'T -> 'T)
                and 'T : (static member (/) : 'T * 'T -> 'T)
                and 'T : (static member (~-) : 'T -> 'T)
                and 'T : (static member Abs : 'T -> 'T)
                and 'T : (static member Pow : 'T * 'T -> 'T)
                and 'T : (static member Sqrt : 'T -> 'T)
                and 'T : (static member op_Explicit : 'T -> float)
                and 'T : comparison> =
    | ZeroMatrix of 'T
    | Matrix of 'T[,]
    /// ZeroMatrix
    static member inline Zero = ZeroMatrix LanguagePrimitives.GenericZero<'T>
    /// Converts Matrix `m` to float[,]
    static member inline op_Explicit(m:Matrix<'T>) =
        match m with
        | Matrix m -> Array2D.map float m
        | ZeroMatrix _ -> Array2D.zeroCreate 0 0
    /// Gets the number of rows of this Matrix
    member inline m.Rows =
        match m with
        | Matrix m -> m.GetLength 0
        | ZeroMatrix _ -> 0
    /// Gets the number of columns of thisMatrix
    member inline m.Cols =
        match m with
        | Matrix m -> m.GetLength 1
        | ZeroMatrix _ -> 0
    /// Gets the entry of this Matrix at row `i` and column `j`
    member inline m.Item
        with get (i, j) =
            match m with
            | Matrix m -> m.[i, j]
            | ZeroMatrix z -> z
    /// Gets a submatrix of this Matrix with the bounds given in `rowStart`, `rowFinish`, `colStart`, `colFinish`
    member inline m.GetSlice(rowStart, rowFinish, colStart, colFinish) =
        match m with
        | Matrix mm ->
            let rowStart = defaultArg rowStart 0
            let rowFinish = defaultArg rowFinish (m.Rows - 1)
            let colStart = defaultArg colStart 0
            let colFinish = defaultArg colFinish (m.Cols - 1)
            Matrix mm.[rowStart..rowFinish, colStart..colFinish]
        | ZeroMatrix _ -> invalidArg "" "Cannot get slice of a ZeroMatrix."
    /// Gets a row subvector of this Matrix with the given row index `row` and column bounds `colStart` and `colFinish`
    member inline m.GetSlice(row, colStart, colFinish) =
        match m with
        | Matrix mm ->
            let colStart = defaultArg colStart 0
            let colFinish = defaultArg colFinish (m.Cols - 1)
            Vector mm.[row, colStart..colFinish]
        | ZeroMatrix _ -> invalidArg "" "Cannot get slice of a ZeroMatrix."
    /// Gets a column subvector of this Matrix with the given column index `col` and row bounds `rowStart` and `rowFinish`
    member inline m.GetSlice(rowStart, rowFinish, col) =
        match m with
        | Matrix mm ->
            let rowStart = defaultArg rowStart 0
            let rowFinish = defaultArg rowFinish (m.Rows - 1)
            Vector mm.[rowStart..rowFinish, col]
        | ZeroMatrix _ -> invalidArg "" "Cannot get slice of a ZeroMatrix."
    /// Gets a string representation of this Matrix that can be pasted into a Mathematica notebook
    member inline m.ToMathematicaString() =
        let sb = System.Text.StringBuilder()
        sb.Append("{") |> ignore
        for i = 0 to m.Rows - 1 do
            sb.Append("{") |> ignore
            for j = 0 to m.Cols - 1 do
                sb.Append(sprintf "%.2f" (float m.[i, j])) |> ignore
                if j <> m.Cols - 1 then sb.Append(", ") |> ignore
            sb.Append("}") |> ignore
            if i <> m.Rows - 1 then sb.Append(", ") |> ignore
        sb.Append("}") |> ignore
        sb.ToString()
    /// Gets a string representation of this Matrix that can be pasted into MATLAB
    member inline m.ToMatlabString() =
        let sb = System.Text.StringBuilder()
        sb.Append("[") |> ignore
        for i = 0 to m.Rows - 1 do
            for j = 0 to m.Cols - 1 do
                sb.Append(sprintf "%.2f" (float m.[i, j])) |> ignore
                if j < m.Cols - 1 then sb.Append(" ") |> ignore
            if i < m.Rows - 1 then sb.Append("; ") |> ignore
        sb.Append("]") |> ignore
        sb.ToString()
    /// Converts this Matrix into a 2d array
    member inline m.ToArray2D() =
        match m with
        | Matrix m -> m
        | ZeroMatrix _ -> Array2D.zeroCreate 0 0
    /// Converts this Matrix into a jagged array, e.g. from Matrix<float> to float[][]
    member inline m.ToArray() =
        let a = m.ToArray2D()
        [|for i = 0 to m.Rows - 1 do yield [|for j = 0 to m.Cols - 1 do yield a.[i, j]|]|]
    /// Creates a copy of this Matrix
    member inline m.Copy() = 
        match m with
        | Matrix m -> Matrix (Array2D.copy m)
        | ZeroMatrix z -> ZeroMatrix z
    /// Gets the trace of this Matrix
    member inline m.GetTrace() =
        match m with
        | Matrix m -> trace m
        | ZeroMatrix z -> z
    /// Gets the transpose of this Matrix
    member inline m.GetTranspose() =
        match m with
        | Matrix m -> Matrix (transpose m)
        | ZeroMatrix z -> ZeroMatrix z
    /// Gets a Vector of the diagonal elements of this Matrix
    member inline m.GetDiagonal() =
        match m with
        | Matrix mm -> 
            if m.Rows <> m.Cols then invalidArg "" "Cannot get the diagonal entries of a nonsquare matrix."
            Array.init m.Rows (fun i -> mm.[i, i])
        | ZeroMatrix z -> [||]
    /// Returns the LU decomposition of this Matrix. The return values are the LU matrix, pivot indices, and a toggle value indicating the number of row exchanges during the decomposition, which is +1 if the number of exchanges were even, -1 if odd.
    member inline m.GetLUDecomposition() =
        match m with
        | Matrix mm ->
            if (m.Rows <> m.Cols) then invalidArg "" "Cannot compute the LU decomposition of a nonsquare matrix."
            let res = Array2D.copy mm
            let perm = Array.init m.Rows (fun i -> i)
            let mutable toggle = LanguagePrimitives.GenericOne<'T>
            for j = 0 to m.Rows - 2 do
                let mutable colmax:'T = abs res.[j, j]
                let mutable prow = j
                for i = j + 1 to m.Rows - 1 do
                    let absresij = abs res.[i, j]
                    if absresij > colmax then
                        colmax <- absresij
                        prow <- i
                if prow <> j then
                    let tmprow = res.[prow, 0..]
                    res.[prow, 0..] <- res.[j, 0..]
                    res.[j, 0..] <- tmprow
                    let tmp = perm.[prow]
                    perm.[prow] <- perm.[j]
                    perm.[j] <- tmp
                    toggle <- -toggle
                for i = j + 1 to m.Rows - 1 do
                    res.[i, j] <- res.[i, j] / res.[j, j]
                    for k = j + 1 to m.Rows - 1 do
                        res.[i, k] <- res.[i, k] - res.[i, j] * res.[j, k]
            Matrix res, perm, toggle
        | ZeroMatrix z -> ZeroMatrix z, [||], LanguagePrimitives.GenericZero<'T>
    /// Gets the determinant of this Matrix
    member inline m.GetDeterminant() =
        match m with
        | Matrix _ ->
            if (m.Rows <> m.Cols) then invalidArg "" "Cannot compute the determinant of a nonsquare matrix."
            let lu, _, toggle = m.GetLUDecomposition()
            toggle * Array.fold (fun s x -> s * x) LanguagePrimitives.GenericOne<'T> (lu.GetDiagonal())
        | ZeroMatrix z -> z
    /// Gets the inverse of this Matrix
    member inline m.GetInverse() =
        match m with
        | Matrix mm ->
            if (m.Rows <> m.Cols) then invalidArg "" "Cannot compute the inverse of a nonsquare matrix."
            let res = Array2D.copy mm
            let lu, perm, _ = m.GetLUDecomposition()
            let b:'T[] = Array.zeroCreate m.Rows
            for i = 0 to m.Rows - 1 do
                for j = 0 to m.Rows - 1 do
                    if i = perm.[j] then
                        b.[j] <- LanguagePrimitives.GenericOne<'T>
                    else
                        b.[j] <- LanguagePrimitives.GenericZero<'T>
                let x = matrixSolveHelper (lu.ToArray2D()) b
                res.[0.., i] <- x
            Matrix res
        | ZeroMatrix z -> ZeroMatrix z
    /// Adds Matrix `a` to Matrix `b`
    static member inline (+) (a:Matrix<'T>, b:Matrix<'T>):Matrix<'T> =
        match a, b with
        | Matrix ma, Matrix mb -> 
            if (a.Rows <> b.Rows) || (a.Cols <> b.Cols) then invalidArg "" "Cannot add matrices of different sizes."
            Matrix (Array2D.init a.Rows a.Cols (fun i j -> ma.[i, j] + mb.[i, j]))
        | Matrix _, ZeroMatrix _ -> a
        | ZeroMatrix _, Matrix _ -> b
        | ZeroMatrix z, ZeroMatrix _ -> ZeroMatrix z
    /// Subtracts Matrix `b` from Matrix `a`
    static member inline (-) (a:Matrix<'T>, b:Matrix<'T>):Matrix<'T> =
        match a, b with
        | Matrix ma, Matrix mb -> 
            if (a.Rows <> b.Rows) || (a.Cols <> b.Cols) then invalidArg "" "Cannot subtract matrices of different sizes."
            Matrix (Array2D.init a.Rows a.Cols (fun i j -> ma.[i, j] - mb.[i, j]))
        | Matrix _, ZeroMatrix _ -> a
        | ZeroMatrix _, Matrix b -> Matrix (Array2D.map (~-) b)
        | ZeroMatrix _, ZeroMatrix _ -> Matrix.Zero
    /// Multiplies Matrix `a` and Matrix `b` (matrix product)
    static member inline (*) (a:Matrix<'T>, b:Matrix<'T>):Matrix<'T> =
        match a, b with
        | Matrix ma, Matrix mb ->
            if (a.Cols <> b.Rows) then invalidArg "" "Cannot multiply two matrices of incompatible sizes."
            Matrix (Array2D.init a.Rows b.Cols (fun i j -> Array.sumBy (fun k -> ma.[i, k] * mb.[k, j]) [|0..(b.Rows - 1)|] ))
        | Matrix _, ZeroMatrix _ -> Matrix.Zero
        | ZeroMatrix _, Matrix _ -> Matrix.Zero
        | ZeroMatrix _, ZeroMatrix _ -> Matrix.Zero
    /// Multiplies Matrix `a` and Matrix `b` element-wise (Hadamard product)
    static member inline (.*) (a:Matrix<'T>, b:Matrix<'T>):Matrix<'T> =
        match a, b with
        | Matrix ma, Matrix mb -> 
            if (a.Rows <> b.Rows) || (a.Cols <> b.Cols) then invalidArg "" "Cannot multiply matrices of different sizes."
            Matrix (Array2D.init a.Rows a.Cols (fun i j -> ma.[i, j] * mb.[i, j]))
        | Matrix _, ZeroMatrix _ -> Matrix.Zero
        | ZeroMatrix _, Matrix _ -> Matrix.Zero
        | ZeroMatrix _, ZeroMatrix _ -> Matrix.Zero
    /// Divides Matrix `a` by Matrix `b` element-wise (Hadamard division)
    static member inline (./) (a:Matrix<'T>, b:Matrix<'T>):Matrix<'T> =
        match a, b with
        | Matrix ma, Matrix mb -> 
            if (a.Rows <> b.Rows) || (a.Cols <> b.Cols) then invalidArg "" "Cannot divide matrices of different sizes."
            Matrix (Array2D.init a.Rows a.Cols (fun i j -> ma.[i, j] / mb.[i, j]))
        | Matrix _, ZeroMatrix _ -> raise (new System.DivideByZeroException("Attempted division by a ZeroMatrix."))
        | ZeroMatrix _, Matrix _ -> Matrix.Zero
        | ZeroMatrix _, ZeroMatrix _ -> raise (new System.DivideByZeroException("Attempted division by a ZeroMatrix."))
    /// Computes the matrix-vector product of Matrix `a` and Vector `b`
    static member inline (*) (a:Matrix<'T>, b:Vector<'T>):Vector<'T> =
        match a, b with
        | Matrix ma, Vector vb ->
            if (a.Cols <> b.Length) then invalidArg "" "Cannot compute the matrix-vector product of a matrix and a vector of incompatible sizes."
            Vector (Array.init a.Rows (fun i -> Array.sumBy (fun j -> ma.[i, j] * vb.[j]) [|0..(b.Length - 1)|] ))
        | Matrix _, ZeroVector _ -> Vector.Zero
        | ZeroMatrix _, Vector _ -> Vector.Zero
        | ZeroMatrix _, ZeroVector _ -> Vector.Zero
    /// Computes the vector-matrix product of Vector `a` and Matrix `b`
    static member inline (*) (a:Vector<'T>, b:Matrix<'T>):Vector<'T> =
        match a, b with
        | Vector va, Matrix mb ->
            if (a.Length <> b.Rows) then invalidArg "" "Cannot compute the vector-matrix product of a vector and matrix of incompatible sizes."
            Vector (Array.init b.Cols (fun i -> Array.sumBy (fun j -> va.[j] * mb.[j, i]) [|0..(a.Length - 1)|]))
        | Vector _, ZeroMatrix _ -> Vector.Zero
        | ZeroVector _, Matrix _ -> Vector.Zero
        | ZeroVector _, ZeroMatrix _ -> Vector.Zero
    /// Adds scalar `b` to each element of Matrix `a`
    static member inline (+) (a:Matrix<'T>, b:'T):Matrix<'T> =
        match a with
        | Matrix a -> Matrix (Array2D.map ((+) b) a)
        | ZeroMatrix z -> invalidArg "" "Unsupported operation. Cannot add a scalar to a ZeroMatrix."
    /// Adds scalar `a` to each element of Matrix `b`
    static member inline (+) (a:'T, b:Matrix<'T>):Matrix<'T> =
        match b with
        | Matrix b -> Matrix (Array2D.map ((+) a) b)
        | ZeroMatrix z -> invalidArg "" "Unsupported operation. Cannot add a scalar to a ZeroMatrix."
    /// Subtracts scalar `b` from each element of Matrix `a`
    static member inline (-) (a:Matrix<'T>, b:'T):Matrix<'T> =
        match a with
        | Matrix a -> Matrix (Array2D.map (fun x -> x - b) a)
        | ZeroMatrix z -> invalidArg "" "Unsupported operation. Cannot subtract a scalar from a ZeroMatrix."
    /// Subtracts each element of of Matrix `b` from scalar `a`
    static member inline (-) (a:'T, b:Matrix<'T>):Matrix<'T> =
        match b with
        | Matrix b -> Matrix (Array2D.map ((-) a) b)
        | ZeroMatrix z -> invalidArg "" "Unsupported operation. Cannot subtract a ZeroMatrix from a scalar."
    /// Multiplies each element of Matrix `a` by scalar `b`
    static member inline (*) (a:Matrix<'T>, b:'T):Matrix<'T> =
        match a with
        | Matrix a -> Matrix (Array2D.map ((*) b) a)
        | ZeroMatrix _ -> Matrix.Zero
    /// Multiplies each element of Matrix `b` by scalar `a`
    static member inline (*) (a:'T, b:Matrix<'T>):Matrix<'T> =
        match b with
        | Matrix b -> Matrix (Array2D.map ((*) a) b)
        | ZeroMatrix _ -> Matrix.Zero
    /// Divides each element of Matrix `a` by scalar `b`
    static member inline (/) (a:Matrix<'T>, b:'T):Matrix<'T> =
        match a with
        | Matrix a -> Matrix (Array2D.map (fun x -> x / b) a)
        | ZeroMatrix _ -> Matrix.Zero
    /// Creates a Matrix whose elements are scalar `a` divided by each element of Matrix `b`
    static member inline (/) (a:'T, b:Matrix<'T>):Matrix<'T> =
        match b with
        | Matrix b -> Matrix (Array2D.map ((/) a) b)
        | ZeroMatrix _ -> raise (new System.DivideByZeroException("Attempted division by a ZeroMatrix."))
    /// Gets the negative of Matrix `a`
    static member inline (~-) (a:Matrix<'T>) =
        match a with
        | Matrix a -> Matrix (Array2D.map (~-) a)
        | ZeroMatrix _ -> Matrix.Zero
    /// Returns the QR decomposition of this Matrix
    member inline m.GetQRDecomposition() =
        match m with
        | ZeroMatrix z -> failwith "Cannot compute the QR decomposition of ZeroMatrix."
        | Matrix mm ->
            let minor (m:_[,]) (d) =
                let rows = Array2D.length1 m
                let cols = Array2D.length2 m
                let ret = Array2D.zeroCreate rows cols
                for i = 0 to d - 1 do
                    ret.[i, i] <- LanguagePrimitives.GenericOne
                Array2D.blit m d d ret d d (rows - d) (cols - d)
                ret
            let identity d = Array2D.init d d (fun i j -> if i = j then LanguagePrimitives.GenericOne else LanguagePrimitives.GenericZero)
            // Householder
            let kmax = -1 + min (m.Rows - 1) m.Cols
            let mutable z = m.Copy()
            let q = Array.create m.Rows Matrix.Zero
            for k = 0 to kmax do
                z <- Matrix (minor (z.ToArray2D()) k)
                let x = z.[*, k]
                let mutable a = x.GetL2Norm()
                if mm.[k, k] > LanguagePrimitives.GenericZero then a <- -a
                let e = (x + Vector.createBasis m.Rows k a).GetUnitVector()
                q.[k] <- Matrix (identity m.Rows) + Matrix (Array2D.init m.Rows m.Rows (fun i j -> -(e.[i] * e.[j] + e.[i] * e.[j])))
                z <- q.[k] * z
            let mutable q' = q.[0]
            for i = 1 to kmax do
                q' <- q.[i] * q'
            q'.GetTranspose(), q' * m
    /// Returns the eigenvalues of this Matrix. (Experimental code, complex eigenvalues are not supported.)
    member inline m.GetEigenvalues() =
        let mutable m' = m.Copy()
        for i = 0 to 20 do
            let q, r = m'.GetQRDecomposition()
            m' <- r * q
        m'.GetDiagonal()

/// Operations on Matrix type. (Implementing functionality similar to Microsoft.FSharp.Collections.Array2D)
[<RequireQualifiedAccess>]
module Matrix =
    /// Creates a Matrix from 2d array `m`
    let inline ofArray2D (m:'T[,]):Matrix<'T> = Matrix m
    /// Creates a Matrix from sequence `s`
    let inline ofSeq (s:seq<seq<'T>>):Matrix<'T> = s |> Array.ofSeq |> Array.ofSeq |> array2D |> Matrix
    /// Converts Matrix `m` to a 2d array, e.g. from Matrix<float> to float[,]
    let inline toArray2D (m:Matrix<'T>):'T[,] = m.ToArray2D()
    /// Converts Matrix `m` to a jagged array, e.g. from Matrix<float> to float[][]
    let inline toArray (m:Matrix<'T>):'T[][] = m.ToArray()
    /// Returns the number of columns in Matrix `m`. This is the same with `Matrix.length2`.
    let inline cols (m:Matrix<'T>):int = m.Cols
    /// Creates a copy of Matrix `m`
    let inline copy (m:Matrix<'T>):Matrix<'T> = m.Copy()
    /// Creates a Matrix with `m` rows, `n` columns, and all entries having value `v`
    let inline create (m:int) (n:int) (v:'T):Matrix<'T> = Matrix (Array2D.create m n v)
    /// Creates a Matrix with `m` rows and all rows equal to array `v`
    let inline createRows (m:int) (v:'T[]):Matrix<'T> = Matrix (array2D (Array.init m (fun _ -> v)))
    /// Gets the LU decomposition of Matrix `m`. The return values are the LU matrix, pivot indices, and a toggle value indicating the number of row exchanges during the decomposition, which is +1 if the number of exchanges were even, -1 if odd.
    let inline decomposeLU (m:Matrix<'T>):(Matrix<'T>*int[]*'T) = m.GetLUDecomposition()
    /// Gets the QR decomposition of Matrix `m`
    let inline decomposeQR (m:Matrix<'T>):(Matrix<'T>*Matrix<'T>) = m.GetQRDecomposition()
    /// Gets the determinant of Matrix `m`
    let inline det (m:Matrix<'T>):'T = m.GetDeterminant()
    /// Gets the diagonal elements of matrix `m`
    let inline diagonal (m:Matrix<'T>):Vector<'T> = m.GetDiagonal() |> Vector.ofSeq
    /// Creates the identity matrix with `m` rows and columns
    let inline identity (m:int):Matrix<'T> =
        Matrix (Array2D.init m m (fun i j -> if i = j then LanguagePrimitives.GenericOne<'T> else LanguagePrimitives.GenericZero<'T>))
    /// Gets the eigenvalues of Matrix `m`
    let inline eigenvalues (m:Matrix<'T>):Vector<'T> = m.GetEigenvalues() |> Vector.ofSeq
    /// Returns the value of the entry with the given indices `i` and `j`
    let inline get (i:int) (j:int) (m:Matrix<'T>):'T = m.[i, j]
    /// Creates a Matrix with `m` rows, `n` columns and a generator function `f` to compute the entries
    let inline init (m:int) (n:int) (f:int->int->'T):Matrix<'T> = Matrix (Array2D.init m n f)
    /// Creates a Matrix with `m` rows and a generator function `f` that gives each row as a an array
    let inline initRows (m:int) (f:int->'T[]):Matrix<'T> = Matrix (array2D (Array.init m f))
    /// Creates a square Matrix with `m` rows and columns and a generator function `f` to compute the elements. Function `f` is used only for populating the diagonal and the upper triangular part of the Matrix, the lower triangular part will be the reflection.
    let inline initSymmetric (m:int) (f:int->int->'T):Matrix<'T> =
        if m = 0 then 
            Matrix.Zero
        else
            let s = Array2D.zeroCreate<'T> m m
            for i = 0 to m - 1 do
                for j = i to m - 1 do
                    s.[i, j] <- f i j
            Matrix (copyUpperToLower s)
    /// Gets the inverse of Matrix `m`
    let inline inverse (m:Matrix<'T>):Matrix<'T> = m.GetInverse()
    /// Returns the number of rows in Matrix `m`. This is the same with `Matrix.rows`.
    let inline length1 (m:Matrix<'T>):int = m.Rows
    /// Returns the number of columns in Matrix `m`. This is the same with `Matrix.cols`.
    let inline length2 (m:Matrix<'T>):int = m.Cols
    /// Creates a Matrix whose entries are the results of applying function `f` to each entry of Matrix `m`
    let inline map (f:'T->'U) (m:Matrix<'T>):Matrix<'U> = m |> toArray2D |> Array2D.map f |> Matrix
    /// Creates a Matrix whose entries are the results of applying function `f` to each entry of Matrix `m`. An element index is also supplied to function `f`.
    let inline mapi (f:int->int->'T->'U) (m:Matrix<'T>):Matrix<'U> = m |> toArray2D |> Array2D.mapi f |> Matrix
    /// Returns the number of rows in Matrix `m`. This is the same with `Matrix.length1`.
    let inline rows (m:Matrix<'T>):int = m.Rows
    /// Solves a system of linear equations ax = b, where the coefficients are given in Matrix `a` and the result vector is Vector `b`. The returned vector will correspond to x.
    let inline solve (a:Matrix<'T>) (b:Vector<'T>):Vector<'T> =
        if a.Cols <> b.Length then invalidArg "" "Cannot solve the system of equations using a matrix and a vector of incompatible sizes."
        let lu, perm, _ = a.GetLUDecomposition()
        let bp = Array.init a.Rows (fun i -> b.[perm.[i]])
        Vector (matrixSolveHelper (lu.ToArray2D()) bp)
    /// Gets the trace of Matrix `m`
    let inline trace (m:Matrix<'T>):'T = m.GetTrace()
    /// Gets the transpose of Matrix `m`
    let inline transpose (m:Matrix<'T>):Matrix<'T> = m.GetTranspose()